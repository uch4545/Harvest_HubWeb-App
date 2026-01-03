using Data;
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.WebApp.Controllers
{
    public class SchemesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchemesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Schemes
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? category = null)
        {
            try
            {
                var query = _context.GovernmentSchemes.AsQueryable();

                // Filter by category
                if (!string.IsNullOrEmpty(category) && category != "All")
                {
                    query = query.Where(s => s.Category == category);
                }

                // Only active schemes by default
                query = query.Where(s => s.Status == "Active" || s.Status == "Upcoming");

                var schemes = await query
                    .OrderByDescending(s => s.IsFeatured)
                    .ThenBy(s => s.DisplayOrder)
                    .ThenByDescending(s => s.CreatedAt)
                    .ToListAsync();

                // Get all categories for filter
                ViewBag.Categories = await _context.GovernmentSchemes
                    .Select(s => s.Category)
                    .Distinct()
                    .ToListAsync();

                ViewBag.SelectedCategory = category ?? "All";

                return View(schemes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Schemes Index error: {ex.Message}");
                return View(new List<GovernmentScheme>());
            }
        }

        // GET: /Schemes/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var scheme = await _context.GovernmentSchemes
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (scheme == null)
                    return NotFound();

                return View(scheme);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Schemes Details error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        #region Admin Actions

        // GET: /Admin/ManageSchemes
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageSchemes()
        {
            try
            {
                var schemes = await _context.GovernmentSchemes
                    .OrderByDescending(s => s.IsFeatured)
                    .ThenBy(s => s.DisplayOrder)
                    .ThenByDescending(s => s.CreatedAt)
                    .ToListAsync();

                return View("~/Views/Admin/ManageSchemes.cshtml", schemes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading schemes. Please try again.";
                Console.WriteLine($"❌ ManageSchemes error: {ex.Message}");
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        // GET: /Schemes/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Schemes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(GovernmentScheme scheme)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(scheme);

                scheme.CreatedAt = DateTime.UtcNow;
                _context.GovernmentSchemes.Add(scheme);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Government scheme added successfully!";
                return RedirectToAction("ManageSchemes");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error adding scheme. Please try again.";
                Console.WriteLine($"❌ Create scheme error: {ex.Message}");
                return View(scheme);
            }
        }

        // GET: /Schemes/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var scheme = await _context.GovernmentSchemes.FindAsync(id);
                if (scheme == null)
                    return NotFound();

                return View(scheme);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading scheme. Please try again.";
                Console.WriteLine($"❌ Edit GET scheme error: {ex.Message}");
                return RedirectToAction("ManageSchemes");
            }
        }

        // POST: /Schemes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, GovernmentScheme scheme)
        {
            try
            {
                if (id != scheme.Id)
                    return NotFound();

                if (!ModelState.IsValid)
                    return View(scheme);

                _context.GovernmentSchemes.Update(scheme);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Government scheme updated successfully!";
                return RedirectToAction("ManageSchemes");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating scheme. Please try again.";
                Console.WriteLine($"❌ Edit POST scheme error: {ex.Message}");
                return View(scheme);
            }
        }

        // POST: /Schemes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var scheme = await _context.GovernmentSchemes.FindAsync(id);
                if (scheme == null)
                    return NotFound();

                _context.GovernmentSchemes.Remove(scheme);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Government scheme deleted successfully!";
                return RedirectToAction("ManageSchemes");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting scheme. Please try again.";
                Console.WriteLine($"❌ Delete scheme error: {ex.Message}");
                return RedirectToAction("ManageSchemes");
            }
        }

        #endregion
    }
}
