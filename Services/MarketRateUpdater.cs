using System;
using System.Linq;
using System.Threading.Tasks;
using Data;
using HarvestHub.WebApp.Data;
using HarvestHub.WebApp.Models;

namespace HarvestHub.WebApp.Services
{
    public class MarketRateUpdater
    {
        private readonly ApplicationDbContext _db;
        private readonly MarketRateService _fetcher;

        public MarketRateUpdater(ApplicationDbContext db, MarketRateService fetcher)
        {
            _db = db;
            _fetcher = fetcher;
        }

        public async Task UpdateMarketRatesAsync()
        {
            try
            {
                var dtos = await _fetcher.FetchLatestRatesAsync();

                if (dtos == null || !dtos.Any())
                {
                    Console.WriteLine("⚠️ No rates fetched! Check data source or API.");
                    return;
                }

                foreach (var dto in dtos)
                {
                    var existing = _db.MarketRates.FirstOrDefault(m => m.CropName == dto.CropName);
                    if (existing != null)
                    {
                        existing.CurrentRate = dto.CurrentRate;
                        existing.LastUpdated = dto.LastUpdated;
                        existing.Unit = dto.Unit;
                        existing.CropNameUrdu = dto.CropNameUrdu;
                    }
                    else
                    {
                        _db.MarketRates.Add(new MarketRate
                        {
                            CropName = dto.CropName,
                            CropNameUrdu = dto.CropNameUrdu,
                            CurrentRate = dto.CurrentRate,
                            Unit = dto.Unit,
                            LastUpdated = dto.LastUpdated
                        });
                    }
                }

                await _db.SaveChangesAsync();
                Console.WriteLine("✅ Market rates updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating market rates: {ex.Message}");
            }
        }
    }
}
