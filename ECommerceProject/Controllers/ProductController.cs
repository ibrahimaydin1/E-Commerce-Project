using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Data;
using ECommerceProject.Models;
using System.Threading.Tasks;

namespace ECommerceProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ürün listesi
        public IActionResult Index(int? categoryId)
        {
            var urunler = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive == true && p.StockQuantity > 0)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                urunler = urunler.Where(p => p.CategoryId == categoryId.Value);
            }

            urunler = urunler.OrderByDescending(p => p.CreatedAt);

            var tumKategoriler = _context.Categories
                .Where(c => c.IsActive == true)
                .ToList();

            ViewBag.Categories = tumKategoriler;
            ViewBag.SelectedCategoryId = categoryId;

            return View(urunler.ToList());
        }

        // Ürün detayları
        public IActionResult Details(int id)
        {
            var urun = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == id && p.IsActive == true);

            if (urun == null)
            {
                return NotFound();
            }

            var benzerUrunler = _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == urun.CategoryId && 
                           p.Id != urun.Id && 
                           p.IsActive == true && 
                           p.StockQuantity > 0)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = benzerUrunler;

            return View(urun);
        }

        // Ürün arama
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var arananUrunler = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive == true && 
                           p.StockQuantity > 0 &&
                           (p.Name.Contains(searchTerm) || 
                            p.Description.Contains(searchTerm)))
                .ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive == true)
                .ToList();

            return View("Index", arananUrunler);
        }
    }
}
