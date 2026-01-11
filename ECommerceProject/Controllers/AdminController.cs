using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Data;
using ECommerceProject.Models;
using ECommerceProject.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // Admin dashboard
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Admin kontrolü
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var model = new AdminDashboardViewModel();
            
            // İstatistikleri hesapla
            model.ToplamUrun = _context.Products.Count();
            model.ToplamSiparis = _context.Orders.Count();
            model.ToplamKullanici = _context.Users.Count();
            model.ToplamKategori = _context.Categories.Count();
            
            // Bugünkü siparişler
            var bugun = DateTime.Today;
            model.BugunkuSiparisler = _context.Orders
                .Where(o => o.OrderDate.Date == bugun)
                .Count();
            
            // Bekleyen siparişler
            model.BekleyenSiparisler = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Pending)
                .Count();
            
            // Son siparişler
            model.SonSiparisler = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            return View(model);
        }

        // Ürünler listesi
        public IActionResult Products()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var urunler = _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(urunler);
        }

        // Ürün ekleme/düzenleme sayfası
        public IActionResult ProductEdit(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var model = new ProductEditViewModel();
            model.Kategoriler = _context.Categories.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList();
            
            if (id.HasValue)
            {
                var urun = _context.Products.Find(id.Value);
                if (urun != null)
                {
                    model.Id = urun.Id;
                    model.Name = urun.Name;
                    model.Description = urun.Description;
                    model.Price = urun.Price;
                    model.StockQuantity = urun.StockQuantity;
                    model.CategoryId = urun.CategoryId;
                    model.SKU = urun.SKU;
                    model.ImageUrl = urun.ImageUrl;
                    model.IsFeatured = urun.IsFeatured;
                }
            }

            return View(model);
        }

        // Ürün kaydetme
        [HttpPost]
        public IActionResult ProductSave(ProductEditViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    // Yeni ürün
                    var urun = new Product();
                    urun.Name = model.Name;
                    urun.Description = model.Description;
                    urun.Price = model.Price;
                    urun.StockQuantity = model.StockQuantity;
                    urun.CategoryId = model.CategoryId;
                    urun.SKU = model.SKU;
                    urun.ImageUrl = model.ImageUrl;
                    urun.IsFeatured = model.IsFeatured;
                    urun.CreatedAt = DateTime.Now;
                    
                    _context.Products.Add(urun);
                }
                else
                {
                    // Mevcut ürün güncelleme
                    var urun = _context.Products.Find(model.Id);
                    if (urun != null)
                    {
                        urun.Name = model.Name;
                        urun.Description = model.Description;
                        urun.Price = model.Price;
                        urun.StockQuantity = model.StockQuantity;
                        urun.CategoryId = model.CategoryId;
                        urun.SKU = model.SKU;
                        urun.ImageUrl = model.ImageUrl;
                        urun.IsFeatured = model.IsFeatured;
                    }
                }
                
                _context.SaveChanges();
                return RedirectToAction("Products");
            }

            model.Kategoriler = _context.Categories.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList();
            return View("ProductEdit", model);
        }

        // Ürün silme
        [HttpPost]
        public IActionResult ProductDelete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var urun = _context.Products.Find(id);
            if (urun != null)
            {
                _context.Products.Remove(urun);
                _context.SaveChanges();
            }

            return RedirectToAction("Products");
        }

        // Siparişler listesi
        public IActionResult Orders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var siparisler = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
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

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var siparis = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == id);

            if (siparis == null)
            {
                return NotFound();
            }

            return View(siparis);
        }

        // Sipariş durum güncelleme
        [HttpPost]
        public IActionResult OrderUpdateStatus(int id, OrderStatus status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var siparis = _context.Orders.Find(id);
            if (siparis != null)
            {
                siparis.OrderStatus = status;
                _context.SaveChanges();

                // Email gönderimi (asenkron)
                if (siparis.User != null && !string.IsNullOrEmpty(siparis.User.Email))
                {
                    _ = Task.Run(async () => {
                        await _emailService.SendOrderStatusUpdateEmailAsync(
                            siparis.User.Email,
                            $"{siparis.User.FirstName} {siparis.User.LastName}",
                            siparis.OrderNumber ?? $"#{siparis.Id}",
                            status.ToString()
                        );
                    });
                }
            }

            return RedirectToAction("OrderDetails", new { id = id });
        }

        // Kullanıcılar listesi
        public IActionResult Users()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null || user.Role != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var kullanicilar = _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToList();

            return View(kullanicilar);
        }
    }

    public class AdminDashboardViewModel
    {
        public int ToplamUrun { get; set; }
        public int ToplamSiparis { get; set; }
        public int ToplamKullanici { get; set; }
        public int ToplamKategori { get; set; }
        public int BugunkuSiparisler { get; set; }
        public int BekleyenSiparisler { get; set; }
        public List<Order>? SonSiparisler { get; set; }
    }

    public class ProductEditViewModel
    {
        public int? Id { get; set; }
        
        [Required(ErrorMessage = "Ürün adı gereklidir")]
        public string? Name { get; set; }
        
        [Required(ErrorMessage = "Ürün açıklaması gereklidir")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "SKU gereklidir")]
        public string? SKU { get; set; }
        
        [Required(ErrorMessage = "Fiyat gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Stok miktarı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya pozitif olmalıdır")]
        public int StockQuantity { get; set; }
        
        [Required(ErrorMessage = "Resim URL gereklidir")]
        public string? ImageUrl { get; set; }
        
        public bool IsFeatured { get; set; }
        
        [Required(ErrorMessage = "Kategori seçimi gereklidir")]
        public int CategoryId { get; set; }
        
        public List<SelectListItem>? Kategoriler { get; set; }
    }
}
