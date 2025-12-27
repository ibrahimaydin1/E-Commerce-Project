using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Data;
using ECommerceProject.Models;
using System.Security.Claims;

namespace ECommerceProject.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Sepet sayfası
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var sepet = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (sepet == null)
            {
                // Yeni sepet oluştur
                sepet = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(sepet);
                _context.SaveChanges();
            }

            return View(sepet);
        }

        // Ürünü sepete ekle
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Lütfen giriş yapın" });
            }

            var urun = _context.Products.Find(productId);
            if (urun == null || urun.StockQuantity < quantity)
            {
                return Json(new { success = false, message = "Ürün stokta yok" });
            }

            var sepet = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (sepet == null)
            {
                sepet = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(sepet);
            }

            var mevcutUrun = sepet.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (mevcutUrun != null)
            {
                mevcutUrun.Quantity += quantity;
            }
            else
            {
                sepet.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = urun.Price,
                    CreatedAt = DateTime.Now
                });
            }

            _context.SaveChanges();

            var sepetAdedi = sepet.CartItems.Sum(ci => ci.Quantity);
            return Json(new { success = true, cartCount = sepetAdedi, message = "Ürün sepete eklendi" });
        }

        // Sepetten ürün çıkar
        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Lütfen giriş yapın" });
            }

            var sepetUrunu = _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (sepetUrunu == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }

            _context.CartItems.Remove(sepetUrunu);
            _context.SaveChanges();

            return Json(new { success = true, message = "Ürün sepetten çıkarıldı" });
        }

        // Sepet miktarını güncelle
        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Lütfen giriş yapın" });
            }

            var sepetUrunu = _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefault(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (sepetUrunu == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı" });
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(sepetUrunu);
            }
            else if (sepetUrunu.Product.StockQuantity >= quantity)
            {
                sepetUrunu.Quantity = quantity;
            }
            else
            {
                return Json(new { success = false, message = "Yetersiz stok" });
            }

            _context.SaveChanges();

            var yeniToplam = sepetUrunu.Quantity * sepetUrunu.UnitPrice;
            return Json(new { success = true, newTotal = yeniToplam.ToString("F2") + " TL" });
        }

        // Sepeti boşalt
        [HttpPost]
        public IActionResult ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Lütfen giriş yapın" });
            }

            var sepet = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (sepet != null)
            {
                _context.CartItems.RemoveRange(sepet.CartItems);
                _context.SaveChanges();
            }

            return Json(new { success = true, message = "Sepet boşaltıldı" });
        }

        // Sepet ürün sayısı (navbar için)
        public IActionResult GetCartCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Json(0);
            }

            var sepet = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (sepet == null)
            {
                return Json(0);
            }

            var sepetAdedi = sepet.CartItems.Sum(ci => ci.Quantity);
            return Json(sepetAdedi);
        }
    }
}
