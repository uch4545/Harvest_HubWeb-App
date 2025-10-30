using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HarvestHub.WebApp.Services
{
    public class MarketRateService
    {
        private readonly HttpClient _http;

        public MarketRateService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MarketRateDto>> FetchLatestRatesAsync()
        {
            try
            {
                var apiUrl = "https://localhost:7290/api/MockMarketRates"; // your mock API

                var resp = await _http.GetAsync(apiUrl);
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync();

                var list = JsonSerializer.Deserialize<List<MarketRateDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Console.WriteLine($"📊 Records fetched from API: {list?.Count}");
                return list ?? new List<MarketRateDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching market rates: {ex.Message}");
                return new List<MarketRateDto>();
            }
        }

    }

    // ✅ DTO (Data Transfer Object)
    public class MarketRateDto
    {
        public string CropName { get; set; } = string.Empty;
        public decimal CurrentRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
