using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ECommerceProject.Models;

namespace ECommerceProject.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category configuration
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product configuration
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cart configuration
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithOne()
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // CartItem configuration
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order configuration
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProductReview configuration
        modelBuilder.Entity<ProductReview>()
            .HasOne(pr => pr.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductReview>()
            .HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Elektronik", Description = "Elektronik ürünler", IsActive = true, CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 2, Name = "Giyim", Description = "Giyim ürünleri", IsActive = true, CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) },
            new Category { Id = 3, Name = "Kitap", Description = "Kitaplar", IsActive = true, CreatedAt = new DateTime(2024, 1, 1), UpdatedAt = new DateTime(2024, 1, 1) }
        );

        // Sample products
        modelBuilder.Entity<Product>().HasData(
            new Product 
            { 
                Id = 1, 
                Name = "Laptop Pro 15", 
                Description = "Yüksek performanslı laptop", 
                SKU = "LP-001", 
                Price = 15999.99m, 
                StockQuantity = 10, 
                CategoryId = 1, 
                IsActive = true, 
                IsFeatured = true,
                ImageUrl = "/images/products/laptop1.jpg",
                CreatedAt = new DateTime(2024, 1, 1), 
                UpdatedAt = new DateTime(2024, 1, 1) 
            },
            new Product 
            { 
                Id = 2, 
                Name = "Akıllı Telefon X", 
                Description = "Son teknoloji akıllı telefon", 
                SKU = "PH-001", 
                Price = 8999.99m, 
                StockQuantity = 25, 
                CategoryId = 1, 
                IsActive = true, 
                IsFeatured = true,
                ImageUrl = "/images/products/phone1.jpg",
                CreatedAt = new DateTime(2024, 1, 1), 
                UpdatedAt = new DateTime(2024, 1, 1) 
            },
            new Product 
            { 
                Id = 3, 
                Name = "T-Shirt Premium", 
                Description = "Kaliteli pamuk t-shirt", 
                SKU = "TS-001", 
                Price = 199.99m, 
                StockQuantity = 50, 
                CategoryId = 2, 
                IsActive = true, 
                IsFeatured = false,
                ImageUrl = "/images/products/tshirt1.jpg",
                CreatedAt = new DateTime(2024, 1, 1), 
                UpdatedAt = new DateTime(2024, 1, 1) 
            },
            new Product 
            { 
                Id = 4, 
                Name = "C# Programlama", 
                Description = "C# programlama kitabı", 
                SKU = "BK-001", 
                Price = 89.99m, 
                StockQuantity = 100, 
                CategoryId = 3, 
                IsActive = true, 
                IsFeatured = false,
                ImageUrl = "/images/products/book1.jpg",
                CreatedAt = new DateTime(2024, 1, 1), 
                UpdatedAt = new DateTime(2024, 1, 1) 
            }
        );
    }
}
