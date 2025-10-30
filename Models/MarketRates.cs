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
        public string CropName { get; set; }

        [Required]
        [Display(Name = "Current Rate (per kg)")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Rate must be positive")]
        public decimal CurrentRate { get; set; }   // ✅ Fixed: Add this
        [Required]
        public decimal Rate { get; set; }
        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now; // ✅ Fixed: Add this
        public DateTime Date { get; internal set; }
    }
}
