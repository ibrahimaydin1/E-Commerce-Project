using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Models;
using ECommerceProject.Data;

namespace ECommerceProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Ana sayfa
    public async Task<IActionResult> Index()
    {
        try
        {
            // Öne çıkan ürünleri getir
            var onecikanUrunler = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsFeatured == true && p.IsActive == true && p.StockQuantity > 0)
                .Take(8)
                .ToList();

            // Yeni ürünleri getir
            var yeniUrunler = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive == true && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToList();

            // Kategorileri getir
            var kategoriler = _context.Categories
                .Where(c => c.IsActive == true && c.ParentCategoryId == null)
                .Take(6)
                .ToList();

            ViewBag.FeaturedProducts = onecikanUrunler;
            ViewBag.NewProducts = yeniUrunler;
            ViewBag.Categories = kategoriler;
        }
        catch
        {
            // Hata durumunda boş listeler gönder
            ViewBag.FeaturedProducts = new List<Product>();
            ViewBag.NewProducts = new List<Product>();
            ViewBag.Categories = new List<Category>();
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
