namespace HarvestHub.WebApp.Models
{
    public class BuyerDashboardViewModel
    {
        public Buyer Buyer { get; set; }
        public List<Crop> Crops { get; set; }
        public List<Order> RecentOrders { get; internal set; }
    }
}
