using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HarvestHub.WebApp.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FarmerId { get; set; }
        
        [ForeignKey("FarmerId")]
        public Farmer Farmer { get; set; }

        // Nullable for non-order notifications (e.g., crop deletion)
        public int? OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        // Type of notification: "Order", "CropDeleted", etc.
        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = "Order";

        [StringLength(200)]
        public string BuyerName { get; set; }

        [StringLength(200)]
        public string CropName { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? TotalPrice { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Message { get; set; }
    }
}
