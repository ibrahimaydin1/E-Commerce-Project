using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Data;
using ECommerceProject.Models;
using ECommerceProject.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PaymentService _paymentService;
        private readonly EmailService _emailService;

        public CheckoutController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, PaymentService paymentService, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
            _emailService = emailService;
        }

        // Ödeme sayfası
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var sepet = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (sepet == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel();
            model.CartItems = sepet.CartItems.ToList();
            model.Subtotal = 0;
            
            // Toplamı hesapla
            foreach (var item in sepet.CartItems)
            {
                model.Subtotal = model.Subtotal + (item.Quantity * item.UnitPrice);
            }
            
            model.ShippingCost = 0; // 500 TL üzeri ücretsiz
            if (model.Subtotal < 500)
            {
                model.ShippingCost = 29.99m;
            }
            
            model.TaxRate = 0.18m;
            model.User = _context.Users.Find(userId);

            return View(model);
        }

        // Ödemeyi tamamla
        [HttpPost]
        public IActionResult ProcessPayment(CheckoutViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return Json(new { success = false, message = "Lütfen giriş yapın" });
            }

            var sepet = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (sepet == null)
            {
                return Json(new { success = false, message = "Sepetiniz boş" });
            }

            try
            {
                // Sipariş oluştur
                var siparis = new Order();
                siparis.UserId = userId;
                siparis.OrderNumber = GenerateOrderNumber();
                siparis.OrderDate = DateTime.Now;
                siparis.OrderStatus = OrderStatus.Pending;
                siparis.PaymentStatus = PaymentStatus.Pending;
                siparis.ShippingFirstName = model.User?.FirstName ?? "";
                siparis.ShippingLastName = model.User?.LastName ?? "";
                siparis.ShippingAddress = model.ShippingAddress;
                siparis.ShippingCity = "İstanbul";
                siparis.ShippingPostalCode = "34000";
                siparis.ShippingCountry = "Türkiye";
                
                // Toplamı hesapla
                decimal toplam = 0;
                foreach (var item in sepet.CartItems)
                {
                    toplam = toplam + (item.Quantity * item.UnitPrice);
                }
                siparis.Subtotal = toplam;
                
                siparis.ShippingAmount = 0;
                if (toplam < 500)
                {
                    siparis.ShippingAmount = 29.99m;
                }
                
                siparis.TaxAmount = toplam * 0.18m;
                siparis.DiscountAmount = 0;
                siparis.TotalAmount = siparis.Subtotal + siparis.ShippingAmount + siparis.TaxAmount - siparis.DiscountAmount;

                // Sipariş ürünlerini ekle
                foreach (var item in sepet.CartItems)
                {
                    var urun = new OrderItem();
                    urun.ProductId = item.ProductId;
                    urun.Quantity = item.Quantity;
                    urun.UnitPrice = item.UnitPrice;
                    urun.ProductName = item.Product.Name;
                    urun.ProductSKU = item.Product.SKU;
                    siparis.OrderItems.Add(urun);

                    // Stok düş
                    var stokUrunu = _context.Products.Find(item.ProductId);
                    if (stokUrunu != null)
                    {
                        stokUrunu.StockQuantity = stokUrunu.StockQuantity - item.Quantity;
                    }
                }

                _context.Orders.Add(siparis);
                _context.SaveChanges();

                return Json(new { 
                    success = true, 
                    orderId = siparis.Id,
                    orderNumber = siparis.OrderNumber,
                    message = "Siparişiniz başarıyla oluşturuldu" 
                });
            }
            catch
            {
                return Json(new { success = false, message = "Sipariş oluşturulurken hata oluştu" });
            }
        }

        // Kredi kartı ödeme işlemi
        [HttpPost]
        public IActionResult ProcessCardPayment(PaymentViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return Json(new { success = false, message = "Lütfen giriş yapın" });
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı" });
            }

            try
            {
                // Ödeme isteği oluştur
                var paymentRequest = new PaymentRequest
                {
                    OrderNumber = model.OrderNumber,
                    Amount = model.Amount,
                    UserId = userId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    IdentityNumber = "12345678901", // Test için
                    Address = model.Address,
                    City = "İstanbul",
                    Country = "Türkiye",
                    ZipCode = "34000",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    CardHolderName = model.CardHolderName,
                    CardNumber = model.CardNumber.Replace(" ", ""),
                    ExpireMonth = model.ExpireMonth,
                    ExpireYear = model.ExpireYear,
                    Cvc = model.Cvc,
                    BasketItems = new List<object>()
                };

                // Test ödemesi yap
                var paymentResponse = _paymentService.ProcessTestPayment(paymentRequest);

                if (paymentResponse.Success)
                {
                    // Siparişi güncelle
                    var siparis = _context.Orders.FirstOrDefault(o => o.OrderNumber == model.OrderNumber);
                    if (siparis != null)
                    {
                        siparis.PaymentStatus = PaymentStatus.Completed;
                        siparis.OrderStatus = OrderStatus.Confirmed;
                        siparis.PaymentId = paymentResponse.PaymentId;
                        _context.SaveChanges();

                        // Email gönderimi (asenkron)
                        var orderUser = _context.Users.Find(userId);
                        if (orderUser != null && !string.IsNullOrEmpty(orderUser.Email))
                        {
                            _ = Task.Run(async () => {
                                await _emailService.SendOrderConfirmationEmailAsync(
                                    orderUser.Email,
                                    $"{orderUser.FirstName} {orderUser.LastName}",
                                    model.OrderNumber,
                                    model.Amount
                                );
                            });
                        }
                    }

                    return Json(new { 
                        success = true, 
                        paymentId = paymentResponse.PaymentId,
                        message = "Ödeme başarıyla tamamlandı" 
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = paymentResponse.Message 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Ödeme hatası: {ex.Message}" 
                });
            }
        }

        // Ödeme sayfası
        public IActionResult Payment(int orderId, string orderNumber, decimal amount, string address)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Siparişi kontrol et
            var siparis = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.UserId == userId);
            if (siparis == null)
            {
                return NotFound();
            }

            ViewBag.OrderId = orderId;
            ViewBag.OrderNumber = orderNumber;
            ViewBag.Amount = amount;
            ViewBag.Address = address;

            return View();
        }

        // Sipariş onay sayfası
        public IActionResult OrderConfirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var siparis = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (siparis == null)
            {
                return NotFound();
            }

            return View(siparis);
        }

        // Sipariş geçmişi
        public IActionResult OrderHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var siparisler = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(siparisler);
        }

        // Sipariş detayı
        public IActionResult OrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var siparis = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (siparis == null)
            {
                return NotFound();
            }

            return View(siparis);
        }

        private string GenerateOrderNumber()
        {
            return "ORD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + new Random().Next(1000, 9999);
        }
    }

    public class CheckoutViewModel
    {
        public List<CartItem>? CartItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxRate { get; set; }
        public ApplicationUser? User { get; set; }
        
        [Required(ErrorMessage = "Teslimat adresi gereklidir")]
        public string ShippingAddress { get; set; } = string.Empty;
        
        public string? BillingAddress { get; set; }
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = "CreditCard";
    }

    public class PaymentViewModel
    {
        public string? OrderNumber { get; set; }
        public decimal Amount { get; set; }
        public string? Address { get; set; }
        
        [Required(ErrorMessage = "Kart sahibi adı gereklidir")]
        public string? CardHolderName { get; set; }
        
        [Required(ErrorMessage = "Kart numarası gereklidir")]
        [CreditCard(ErrorMessage = "Geçerli bir kart numarası girin")]
        public string? CardNumber { get; set; }
        
        [Required(ErrorMessage = "Son kullanma ayı gereklidir")]
        [Range(1, 12, ErrorMessage = "Geçerli bir ay girin")]
        public string? ExpireMonth { get; set; }
        
        [Required(ErrorMessage = "Son kullanma yılı gereklidir")]
        [Range(2024, 2030, ErrorMessage = "Geçerli bir yıl girin")]
        public string? ExpireYear { get; set; }
        
        [Required(ErrorMessage = "CVC gereklidir")]
        [Range(100, 999, ErrorMessage = "Geçerli bir CVC girin")]
        public string? Cvc { get; set; }
    }
}
