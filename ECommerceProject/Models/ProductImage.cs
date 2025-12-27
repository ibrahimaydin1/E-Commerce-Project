using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Alt text en fazla 200 karakter olabilir")]
        public string? AltText { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public int ProductId { get; set; }

        public virtual Product Product { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
