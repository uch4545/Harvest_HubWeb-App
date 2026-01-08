using System.ComponentModel.DataAnnotations;

namespace HarvestHub.WebApp.Models
{
    public class FertilizerCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? NameUrdu { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; } // Emoji or icon class

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<FertilizerProduct>? Products { get; set; }
    }
}
