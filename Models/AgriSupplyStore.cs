using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HarvestHub.WebApp.Models
{
    public class AgriSupplyStore
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string StoreName { get; set; }

        [StringLength(50)]
        public string? StoreType { get; set; } = "All"; // All, Fertilizers, Pesticides, Seeds

        [StringLength(100)]
        public string? OwnerName { get; set; }

        [Required]
        public int CityId { get; set; }

        [ForeignKey("CityId")]
        public City City { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }

        [Required]
        [StringLength(20)]
        [Phone]
        public string ContactNumber { get; set; }

        [StringLength(20)]
        [Phone]
        public string? WhatsAppNumber { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool IsVerified { get; set; } = false;

        public decimal? Rating { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<StoreProduct>? StoreProducts { get; set; }
    }
}
