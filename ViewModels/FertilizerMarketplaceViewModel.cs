using HarvestHub.WebApp.Models;

namespace HarvestHub.WebApp.ViewModels
{
    public class FertilizerMarketplaceViewModel
    {
        public List<FertilizerCategory> Categories { get; set; } =  new List<FertilizerCategory>();
        public List<FertilizerProduct> Products { get; set; } = new List<FertilizerProduct>();
        public List<City> Cities { get; set; } = new List<City>();
        
        public int? SelectedCategoryId { get; set; }
        public int? SelectedCityId { get; set; }
        public string? SearchQuery { get; set; }
    }
}
