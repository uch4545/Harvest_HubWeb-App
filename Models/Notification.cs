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

        [Required]
        public int OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        [StringLength(200)]
        public string BuyerName { get; set; }

        [Required]
        [StringLength(200)]
        public string CropName { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Message { get; set; }
    }
}
