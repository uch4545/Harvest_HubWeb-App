using HarvestHub.WebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace HarvestHub.WebApp.ViewModels
{
    public class CreateStoreViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Store name is required")]
        [StringLength(200)]
        public string StoreName { get; set; }

        [StringLength(50)]
        public string? StoreType { get; set; } = "All";

        [StringLength(100)]
        public string? OwnerName { get; set; }

        [Required(ErrorMessage = "City is required")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
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

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; } = true;

        // For dropdown
        public List<City> Cities { get; set; } = new List<City>();
    }
}
