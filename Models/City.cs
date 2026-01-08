using System.ComponentModel.DataAnnotations;

namespace HarvestHub.WebApp.Models
{
    public class City
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? NameUrdu { get; set; }

        [Required]
        [StringLength(100)]
        public string District { get; set; }

        [Required]
        [StringLength(50)]
        public string Province { get; set; } = "Punjab";

        [StringLength(100)]
        public string? Region { get; set; } // e.g., "South Punjab"

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<AgriSupplyStore>? Stores { get; set; }
    }
}
