using System;
using System.ComponentModel.DataAnnotations;

namespace HarvestHub.WebApp.Models
{
    public class CropRate
    {
        public string CropName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class MarketRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Crop Name")]
        public string CropName { get; set; } = string.Empty;

        [Display(Name = "Urdu Name")]
        public string? CropNameUrdu { get; set; }

        [Required]
        [Display(Name = "Current Rate (PKR)")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Rate must be positive")]
        public decimal CurrentRate { get; set; }

        [Display(Name = "Unit")]
        public string Unit { get; set; } = "40 Kg";

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Category for grouping (Grain, Pulse, Vegetable, Fruit, etc.)
        public string? Category { get; set; }
    }
}

