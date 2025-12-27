using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceProject.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir")]
        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Kupon kodu en fazla 20 karakter olabilir")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200, ErrorMessage = "Kupon açıklaması en fazla 200 karakter olabilir")]
        public string Description { get; set; } = string.Empty;

        public enum DiscountType
        {
            Percentage = 0,
            FixedAmount = 1
        }

        public DiscountType Type { get; set; } = DiscountType.Percentage;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "İndirim değeri 0'dan büyük olmalıdır")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumOrderAmount { get; set; } = 0;

        public int UsageLimit { get; set; } = 0; // 0 means unlimited
        public int UsedCount { get; set; } = 0;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
