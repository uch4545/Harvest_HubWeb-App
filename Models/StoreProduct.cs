using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HarvestHub.WebApp.Models
{
    public class StoreProduct
    {
        public int Id { get; set; }

        [Required]
        public int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public AgriSupplyStore Store { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public FertilizerProduct Product { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public bool IsInStock { get; set; } = true;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
