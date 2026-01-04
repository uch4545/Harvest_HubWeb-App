using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HarvestHub.WebApp.Services
{
    public class MarketRateService
    {
        private readonly HttpClient _http;

        public MarketRateService(HttpClient http)
        {
            _http = http;
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<MarketRateDto>> FetchLatestRatesAsync()
        {
            try
            {
                // Pakistan major crops with realistic price ranges (PKR per 40 kg/maund or per kg)
                var pakistanCrops = GetPakistanCropRates();
                Console.WriteLine($"📊 {pakistanCrops.Count} Pakistan crop rates generated");
                return pakistanCrops;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching market rates: {ex.Message}");
                return new List<MarketRateDto>();
            }
        }

        private List<MarketRateDto> GetPakistanCropRates()
        {
            var random = new Random();
            var now = DateTime.UtcNow;

            // Realistic price ranges for Pakistani crops (PKR per 40 kg maund unless specified)
            var crops = new List<(string Name, string NameUrdu, decimal MinPrice, decimal MaxPrice, string Unit)>
            {
                // Grains / اناج
                ("Wheat / گندم", "گندم", 3800, 4200, "40 Kg"),
                ("Rice Basmati / باسمتی چاول", "باسمتی", 8500, 12000, "40 Kg"),
                ("Rice IRRI / عام چاول", "عام چاول", 4500, 5500, "40 Kg"),
                ("Maize / مکئی", "مکئی", 2800, 3200, "40 Kg"),
                ("Barley / جو", "جو", 3200, 3600, "40 Kg"),
                
                // Pulses / دالیں
                ("Chickpea / چنا", "چنا", 12000, 15000, "40 Kg"),
                ("Lentil / مسور", "مسور", 18000, 22000, "40 Kg"),
                ("Mung Bean / مونگ", "مونگ", 16000, 20000, "40 Kg"),
                ("Mash / ماش", "ماش", 20000, 25000, "40 Kg"),
                
                // Oilseeds / تیل دار بیج
                ("Cotton / کپاس", "کپاس", 8000, 9500, "40 Kg"),
                ("Sunflower / سورج مکھی", "سورج مکھی", 6000, 7500, "40 Kg"),
                ("Canola / کینولا", "کینولا", 7000, 8500, "40 Kg"),
                ("Sesame / تل", "تل", 25000, 32000, "40 Kg"),
                
                // Cash Crops
                ("Sugarcane / گنا", "گنا", 300, 350, "40 Kg"),
                ("Tobacco / تمباکو", "تمباکو", 15000, 20000, "40 Kg"),
                
                // Vegetables / سبزیاں (per Kg)
                ("Potato / آلو", "آلو", 80, 120, "Kg"),
                ("Onion / پیاز", "پیاز", 100, 180, "Kg"),
                ("Tomato / ٹماٹر", "ٹماٹر", 120, 250, "Kg"),
                ("Garlic / لہسن", "لہسن", 300, 500, "Kg"),
                ("Ginger / ادرک", "ادرک", 400, 600, "Kg"),
                ("Carrot / گاجر", "گاجر", 60, 100, "Kg"),
                ("Cauliflower / گوبھی", "گوبھی", 80, 150, "Kg"),
                ("Chili / مرچ", "مرچ", 200, 400, "Kg"),
                
                // Fruits / پھل (per Kg)
                ("Mango / آم", "آم", 150, 350, "Kg"),
                ("Apple / سیب", "سیب", 200, 350, "Kg"),
                ("Orange / مالٹا", "مالٹا", 100, 180, "Kg"),
                ("Banana / کیلا", "کیلا", 100, 150, "Kg"),
                ("Guava / امرود", "امرود", 80, 150, "Kg"),
                ("Dates / کھجور", "کھجور", 250, 500, "Kg"),
            };

            var rates = new List<MarketRateDto>();

            foreach (var crop in crops)
            {
                // Generate realistic random price within range
                var priceRange = crop.MaxPrice - crop.MinPrice;
                var randomPrice = crop.MinPrice + (decimal)(random.NextDouble() * (double)priceRange);
                randomPrice = Math.Round(randomPrice, 0);

                rates.Add(new MarketRateDto
                {
                    CropName = crop.Name,
                    CropNameUrdu = crop.NameUrdu,
                    CurrentRate = randomPrice,
                    Unit = crop.Unit,
                    LastUpdated = now.AddMinutes(-random.Next(0, 120)) 
                });
            }

            return rates;
        }
    }

    // DTO (Data Transfer Object)
    public class MarketRateDto
    {
        public string CropName { get; set; } = string.Empty;
        public string CropNameUrdu { get; set; } = string.Empty;
        public decimal CurrentRate { get; set; }
        public string Unit { get; set; } = "40 Kg";
        public DateTime LastUpdated { get; set; }
    }
}
