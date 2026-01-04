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
            try
            {
                var rates = await _context.MarketRates
                    .OrderBy(r => r.CropName)
                    .ToListAsync();

                return View(rates);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading rates. Please try again.";
                Console.WriteLine($"❌ MarketRates Index error: {ex.Message}");
                return View(new List<MarketRate>());
            }
        }

        // POST: /MarketRates/UpdateNow
        [HttpPost]
        public async Task<IActionResult> UpdateNow()
        {
            try
            {
                // Check if rates were already updated today (Pakistan time)
                var pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                var nowPakistan = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone);
                var todayStartPakistan = nowPakistan.Date; // Midnight of today
                
                var latestRate = await _context.MarketRates
                    .OrderByDescending(r => r.LastUpdated)
                    .FirstOrDefaultAsync();
                
                if (latestRate != null)
                {
                    var lastUpdatePakistan = TimeZoneInfo.ConvertTimeFromUtc(latestRate.LastUpdated, pakistanTimeZone);
                    
                    // If the last update was today (same date), don't update again
                    if (lastUpdatePakistan.Date == todayStartPakistan)
                    {
                        var nextUpdateTime = todayStartPakistan.AddDays(1).ToString("dd MMM yyyy, 12:00 AM");
                        TempData["Info"] = $"⏰ Rates are already up-to-date for today. Next update available after midnight: {nextUpdateTime}";
                        return RedirectToAction("Index");
                    }
                }
                
                var latestRates = await _service.FetchLatestRatesAsync();

                if (!latestRates.Any())
                {
                    TempData["Error"] = "No rates available from the service.";
                    return RedirectToAction("Index");
                }

                // Clear old data and add fresh Pakistan crop rates
                var existingRates = await _context.MarketRates.ToListAsync();
                _context.MarketRates.RemoveRange(existingRates);
                
                foreach (var dto in latestRates)
                {
                    _context.MarketRates.Add(new MarketRate
                    {
                        CropName = dto.CropName,
                        CropNameUrdu = dto.CropNameUrdu,
                        CurrentRate = dto.CurrentRate,
                        Unit = dto.Unit,
                        LastUpdated = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"✅ {latestRates.Count} Pakistan crop rates updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Error updating rates: {ex.Message}";
                Console.WriteLine($"❌ UpdateNow error: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        #region Admin Actions

        // GET: /Admin/ManageRates
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRates()
        {
            try
            {
                var rates = await _context.MarketRates
                    .OrderBy(r => r.CropName)
                    .ToListAsync();

                return View("~/Views/Admin/ManageRates.cshtml", rates);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading rates. Please try again.";
                Console.WriteLine($"❌ ManageRates error: {ex.Message}");
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        // GET: /MarketRates/Create
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /MarketRates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(MarketRate rate)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(rate);

                rate.LastUpdated = DateTime.UtcNow;
                _context.MarketRates.Add(rate);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Market rate added successfully!";
                return RedirectToAction("ManageRates");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error adding rate. Please try again.";
                Console.WriteLine($"❌ Create error: {ex.Message}");
                return View(rate);
            }
        }

        // GET: /MarketRates/Edit/5
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rate = await _context.MarketRates.FindAsync(id);
                if (rate == null)
                    return NotFound();

                return View(rate);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading rate. Please try again.";
                Console.WriteLine($"❌ Edit GET error: {ex.Message}");
                return RedirectToAction("ManageRates");
            }
        }

        // POST: /MarketRates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, MarketRate rate)
        {
            try
            {
                if (id != rate.Id)
                    return NotFound();

                if (!ModelState.IsValid)
                    return View(rate);

                rate.LastUpdated = DateTime.UtcNow;
                _context.MarketRates.Update(rate);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Market rate updated successfully!";
                return RedirectToAction("ManageRates");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating rate. Please try again.";
                Console.WriteLine($"❌ Edit POST error: {ex.Message}");
                return View(rate);
            }
        }

        // POST: /MarketRates/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rate = await _context.MarketRates.FindAsync(id);
                if (rate == null)
                    return NotFound();

                _context.MarketRates.Remove(rate);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Market rate deleted successfully!";
                return RedirectToAction("ManageRates");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting rate. Please try again.";
                Console.WriteLine($"❌ Delete error: {ex.Message}");
                return RedirectToAction("ManageRates");
            }
        }

        #endregion
    }
}
