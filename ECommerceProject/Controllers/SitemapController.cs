using Microsoft.AspNetCore.Mvc;
using ECommerceProject.Data;
using ECommerceProject.Models;
using System.Text;
using System.Xml;

namespace ECommerceProject.Controllers
{
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SitemapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var urls = new List<SitemapUrl>();

            // Ana sayfa
            urls.Add(new SitemapUrl
            {
                Loc = baseUrl,
                LastMod = DateTime.Now,
                ChangeFreq = "daily",
                Priority = "1.0"
            });

            // Ürünler sayfası
            urls.Add(new SitemapUrl
            {
                Loc = $"{baseUrl}/Product/Index",
                LastMod = DateTime.Now,
                ChangeFreq = "daily",
                Priority = "0.9"
            });

            // Kategoriler
            var kategoriler = _context.Categories.Where(c => c.IsActive).ToList();
            foreach (var kategori in kategoriler)
            {
                urls.Add(new SitemapUrl
                {
                    Loc = $"{baseUrl}/Product/Index?categoryId={kategori.Id}",
                    LastMod = kategori.UpdatedAt != null ? kategori.UpdatedAt : kategori.CreatedAt,
                    ChangeFreq = "weekly",
                    Priority = "0.8"
                });
            }

            // Ürünler
            var urunler = _context.Products.Where(p => p.IsActive).ToList();
            foreach (var urun in urunler)
            {
                urls.Add(new SitemapUrl
                {
                    Loc = $"{baseUrl}/Product/Detail/{urun.Id}",
                    LastMod = urun.UpdatedAt != null ? urun.UpdatedAt : urun.CreatedAt,
                    ChangeFreq = "weekly",
                    Priority = "0.7"
                });
            }

            // Diğer sayfalar
            urls.Add(new SitemapUrl
            {
                Loc = $"{baseUrl}/About",
                LastMod = DateTime.Now,
                ChangeFreq = "monthly",
                Priority = "0.5"
            });

            urls.Add(new SitemapUrl
            {
                Loc = $"{baseUrl}/Contact",
                LastMod = DateTime.Now,
                ChangeFreq = "monthly",
                Priority = "0.5"
            });

            var sitemap = GenerateSitemap(urls);
            return Content(sitemap, "application/xml");
        }

        private string GenerateSitemap(List<SitemapUrl> urls)
        {
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                foreach (var url in urls)
                {
                    writer.WriteStartElement("url");
                    
                    writer.WriteStartElement("loc");
                    writer.WriteString(url.Loc);
                    writer.WriteEndElement();

                    writer.WriteStartElement("lastmod");
                    writer.WriteString(url.LastMod.ToString("yyyy-MM-dd"));
                    writer.WriteEndElement();

                    writer.WriteStartElement("changefreq");
                    writer.WriteString(url.ChangeFreq);
                    writer.WriteEndElement();

                    writer.WriteStartElement("priority");
                    writer.WriteString(url.Priority);
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();

                return stream.ToString();
            }
        }
    }

    public class SitemapUrl
    {
        public string Loc { get; set; } = string.Empty;
        public DateTime LastMod { get; set; }
        public string ChangeFreq { get; set; } = "weekly";
        public string Priority { get; set; } = "0.5";
    }
}
