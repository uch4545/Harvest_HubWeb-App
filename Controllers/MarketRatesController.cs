using Data;
using HarvestHub.WebApp.Data;
using HarvestHub.WebApp.Models;
using HarvestHub.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.WebApp.Controllers
{
    public class MarketRatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MarketRateService _service;

        public MarketRatesController(ApplicationDbContext context, MarketRateService service)
        {
            _context = context;
            _service = service;
        }

        // GET: /MarketRates
        public async Task<IActionResult> Index()
        {
            // ✅ Always fetch fresh data from database
            var rates = await _context.MarketRates
                .OrderBy(r => r.CropName)
                .ToListAsync();

            return View(rates);
        }

        // POST: /MarketRates/UpdateNow
        [HttpPost]
        public async Task<IActionResult> UpdateNow()
        {
            try
            {
                var latestRates = await _service.FetchLatestRatesAsync();

                foreach (var dto in latestRates)
                {
                    var existing = await _context.MarketRates
                        .FirstOrDefaultAsync(r => r.CropName.ToLower() == dto.CropName.ToLower());

                    if (existing != null)
                    {
                        existing.CurrentRate = dto.CurrentRate;
                        existing.LastUpdated = dto.LastUpdated;
                        _context.MarketRates.Update(existing);
                    }
                    else
                    {
                        var newRate = new MarketRate
                        {
                            CropName = dto.CropName,
                            CurrentRate = dto.CurrentRate,
                            LastUpdated = dto.LastUpdated
                        };
                        _context.MarketRates.Add(newRate);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ {latestRates.Count} market rates updated successfully!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"❌ Error updating rates: {ex.Message}";
            }

            // ✅ Force fresh reload
            return RedirectToAction("Index", new { t = DateTime.Now.Ticks });
        }
    }
}
