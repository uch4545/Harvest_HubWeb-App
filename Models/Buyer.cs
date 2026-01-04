using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HarvestHub.WebApp.Models
{
    public class Buyer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string CNIC { get; set; }

        public string PhoneNumber { get; set; }
        
        public string? ProfileImagePath { get; set; }

        public string PasswordHash { get; set; }

        // 👇 Foreign Key to AspNetUsers
        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
        // ✅ Orders relation
        public ICollection<Order> Orders { get; set; }
    }
}
