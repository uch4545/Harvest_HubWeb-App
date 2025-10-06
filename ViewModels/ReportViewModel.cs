using HarvestHub.WebApp.Models;

namespace Harvest_Hub.ViewModels
{
    public class ReportViewModel
    {
        public LabReport Report { get; set; }
        public Crop Crop { get; set; }
        public Laboratory Laboratory { get; set; }
    }

}
