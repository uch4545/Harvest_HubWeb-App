using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HarvestHub.WebApp.Models
{
    public class FertilizerProduct
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? NameUrdu { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public FertilizerCategory? Category { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? ManufacturerName { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? PackageSize { get; set; } // e.g., "50kg", "1 liter"

        [StringLength(20)]
        public string? Unit { get; set; } // e.g., "kg", "liter", "bag"

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(1000)]
        public string? UsageInstructions { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<StoreProduct>? StoreProducts { get; set; }
    }
}
