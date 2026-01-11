using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [PersonalData]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [PersonalData]
        public string? LastName { get; set; }

        [PersonalData]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        [PersonalData]
        public string? Gender { get; set; }

        [StringLength(20)]
        [PersonalData]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        [PersonalData]
        public string? Address { get; set; }

        [StringLength(100)]
        [PersonalData]
        public string? City { get; set; }

        [StringLength(20)]
        [PersonalData]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        [PersonalData]
        public string? Country { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Role { get; set; }
    }
}
