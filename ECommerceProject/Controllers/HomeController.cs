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

    public IActionResult Index()
    {
        ViewData["Title"] = "E-Commerce - Online Alışveriş Sitesi";
        ViewData["Description"] = "En iyi ürünler uygun fiyatlarla. Online alışveriş deneyiminizi keşfedin.";
        ViewData["Keywords"] = "e-ticaret, alışveriş, online satış, ürünler, indirim, kampanya";
        ViewData["ImageUrl"] = "https://localhost:5214/images/logo.png";

        var structuredData = @"
        {
            ""@context"": ""https://schema.org"",
            ""@type"": ""WebSite"",
            ""name"": ""E-Commerce"",
            ""url"": ""https://localhost:5214"",
            ""description"": ""Online alışveriş sitesi"",
            ""potentialAction"": {
                ""@type"": ""SearchAction"",
                ""target"": ""https://localhost:5214/Product/Search?searchTerm={search_term_string}"",
                ""query-input"": ""required name=search_term_string""
            }
        }";
        ViewData["StructuredData"] = structuredData;

        try
        {
            var onecikanUrunler = new List<Product>();
            var tumUrunler = _context.Products.ToList();
            
            for(int i = 0; i < tumUrunler.Count; i++)
            {
                var urun = tumUrunler[i];
                if(urun.IsFeatured == true && urun.IsActive == true && urun.StockQuantity > 0)
                {
                    onecikanUrunler.Add(urun);
                }
                if(onecikanUrunler.Count >= 8) break;
            }

            var yeniUrunler = new List<Product>();
            var siraliUrunler = _context.Products.OrderByDescending(p => p.CreatedAt).ToList();
            
            foreach(var urun in siraliUrunler)
            {
                if(urun.IsActive == true && urun.StockQuantity > 0)
                {
                    yeniUrunler.Add(urun);
                }
                if(yeniUrunler.Count >= 8) break;
            }

            var kategoriler = new List<Category>();
            var tumKategoriler = _context.Categories.ToList();
            
            foreach(var kategori in tumKategoriler)
            {
                if(kategori.IsActive == true && kategori.ParentCategoryId == null)
                {
                    kategoriler.Add(kategori);
                }
                if(kategoriler.Count >= 6) break;
            }

            ViewBag.FeaturedProducts = onecikanUrunler;
            ViewBag.NewProducts = yeniUrunler;
            ViewBag.Categories = kategoriler;
        }
        catch(Exception ex)
        {
            Console.WriteLine("Hata olustu: " + ex.Message);
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

    public IActionResult AccessDenied()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
