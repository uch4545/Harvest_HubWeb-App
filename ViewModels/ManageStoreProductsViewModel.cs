using HarvestHub.WebApp.Models;

namespace HarvestHub.WebApp.ViewModels
{
    public class ManageStoreProductsViewModel
    {
        public AgriSupplyStore Store { get; set; }
        public List<StoreProduct> StoreProducts { get; set; } = new List<StoreProduct>();
        public List<FertilizerProduct> AvailableProducts { get; set; } = new List<FertilizerProduct>();
        
        public int SelectedProductId { get; set; }
        public decimal Price { get; set; }
    }
}
