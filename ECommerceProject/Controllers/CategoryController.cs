using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Data;
using ECommerceProject.Models;
using System.Threading.Tasks;

namespace ECommerceProject.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Category/Index
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                    .ThenInclude(p => p.ProductImages)
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                    .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: /Category/Products/5
        public async Task<IActionResult> Products(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == id && p.IsActive && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Category = category;
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            return View(products);
        }
    }
}
