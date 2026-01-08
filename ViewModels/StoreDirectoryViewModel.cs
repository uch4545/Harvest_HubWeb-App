using HarvestHub.WebApp.Models;

namespace HarvestHub.WebApp.ViewModels
{
    public class StoreDirectoryViewModel
    {
        public List<AgriSupplyStore> Stores { get; set; } = new List<AgriSupplyStore>();
        public List<City> Cities { get; set; } = new List<City>();
        
        public int? SelectedCityId { get; set; }
        public string? SearchQuery { get; set; }
    }
}
