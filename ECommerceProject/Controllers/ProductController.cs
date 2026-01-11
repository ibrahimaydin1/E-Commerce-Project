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

        public IActionResult Details(int id)
        {
            var tumUrunler = _context.Products.ToList();
            Product urun = null;
            
            foreach(var u in tumUrunler)
            {
                if(u.Id == id && u.IsActive == true)
                {
                    urun = u;
                    break;
                }
            }

            if (urun == null)
            {
                return NotFound();
            }

            ViewData["Title"] = urun.Name + " - E-Commerce";
            ViewData["Description"] = urun.Description.Substring(0, Math.Min(urun.Description.Length, 160)) + "...";
            ViewData["Keywords"] = urun.Name + ", " + (urun.Category != null ? urun.Category.Name : "") + ", e-ticaret, alışveriş, " + urun.SKU;
            ViewData["ImageUrl"] = urun.ImageUrl ?? "/images/default-product.jpg";

            var structuredData = "{";
            structuredData += "\"@context\": \"https://schema.org\",";
            structuredData += "\"@type\": \"Product\",";
            structuredData += "\"name\": \"" + urun.Name + "\",";
            structuredData += "\"description\": \"" + urun.Description + "\",";
            structuredData += "\"image\": \"" + ViewData["ImageUrl"] + "\",";
            structuredData += "\"offers\": {";
            structuredData += "\"@type\": \"Offer\",";
            structuredData += "\"price\": \"" + urun.Price.ToString("F2") + "\",";
            structuredData += "\"priceCurrency\": \"TRY\",";
            structuredData += "\"availability\": \"" + (urun.StockQuantity > 0 ? "InStock" : "OutOfStock") + "\"";
            structuredData += "},";
            structuredData += "\"brand\": {";
            structuredData += "\"@type\": \"Brand\",";
            structuredData += "\"name\": \"E-Commerce\"";
            structuredData += "},";
            structuredData += "\"category\": \"" + (urun.Category != null ? urun.Category.Name : "") + "\"";
            structuredData += "}";
            ViewData["StructuredData"] = structuredData;

            var benzerUrunler = new List<Product>();
            var kategoriUrunleri = _context.Products.ToList();
            
            foreach(var u in kategoriUrunleri)
            {
                if(u.CategoryId == urun.CategoryId && 
                   u.Id != urun.Id && 
                   u.IsActive == true && 
                   u.StockQuantity > 0)
                {
                    benzerUrunler.Add(u);
                }
                if(benzerUrunler.Count >= 4) break;
            }

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
