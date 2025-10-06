using Data; // your DbContext
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.Controllers
{
    [Authorize(Roles = "Admin")] // ✅ Only Admin role can access
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        #region ---------------- Dashboard ----------------
        // ✅ Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalFarmers = await _context.Farmers.CountAsync();
            var totalBuyers = await _context.Buyers.CountAsync();
            var totalCrops = await _context.Crops.CountAsync();
            var totalLabs = await _context.Laboratories.CountAsync();

            var pendingDocs = await _context.VerificationDocument
                                            .CountAsync(d => d.Status == VerificationStatus.Pending);
            var approvedDocs = await _context.VerificationDocument
                                             .CountAsync(d => d.Status == VerificationStatus.Approved);
            var rejectedDocs = await _context.VerificationDocument
                                             .CountAsync(d => d.Status == VerificationStatus.Rejected);

            ViewBag.TotalFarmers = totalFarmers;
            ViewBag.TotalBuyers = totalBuyers;
            ViewBag.TotalCrops = totalCrops;
            ViewBag.TotalLabs = totalLabs;
            ViewBag.PendingDocs = pendingDocs;
            ViewBag.ApprovedDocs = approvedDocs;
            ViewBag.RejectedDocs = rejectedDocs;

            return View();
        }


        // ✅ Manage all users
        public async Task<IActionResult> ManageUsers()
        {
            var farmers = await _context.Farmers.ToListAsync();
            var buyers = await _context.Buyers.ToListAsync();
            ViewBag.Farmers = farmers;
            ViewBag.Buyers = buyers;
            return View();
        }

        // ✅ Manage all products
        public async Task<IActionResult> ManageProducts()
        {
            var crops = await _context.Crops.Include(c => c.Farmer).ToListAsync();
            return View(crops);
        }

        // ✅ Verify documents
        public async Task<IActionResult> VerifyDocuments()
        {
            var docs = await _context.VerificationDocument
                          .Include(v => v.User) // ✅ now valid
                          .OrderByDescending(v => v.SubmittedAt)
                          .ToListAsync();


            return View(docs);
        }

        #endregion ---------------- Dashboard ----------------

        #region ---------------- ManageLabs ----------------

        // ✅ List All Labs
        public async Task<IActionResult> ManageLabs()
        {
            var labs = await _context.Laboratories.ToListAsync();
            return View(labs);
        }

        // ✅ Create Lab (GET)
        public IActionResult CreateLab()
        {
            return View();
        }

        // ✅ Create Lab (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLab(Laboratory lab)
        {
            if (!ModelState.IsValid) return View(lab);

            _context.Laboratories.Add(lab);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Lab added successfully!";
            return RedirectToAction("ManageLabs");
        }

        // ✅ Verify Lab
        [HttpPost]
        public async Task<IActionResult> VerifyLab(int id)
        {
            var lab = await _context.Laboratories.FindAsync(id);
            if (lab != null)
            {
                lab.IsVerified = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageLabs");
        }
        // ✅ Edit Lab (GET)
        public async Task<IActionResult> EditLab(int id)
        {
            var lab = await _context.Laboratories.FindAsync(id);
            if (lab == null) return NotFound();
            return View(lab);
        }

        // ✅ Edit Lab (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLab(Laboratory lab)
        {
            if (!ModelState.IsValid) return View(lab);

            _context.Laboratories.Update(lab);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Lab updated successfully!";
            return RedirectToAction("ManageLabs");
        }

        // ✅ Delete Lab
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLab(int id)
        {
            var lab = await _context.Laboratories.FindAsync(id);
            if (lab == null) return NotFound();

            _context.Laboratories.Remove(lab);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Lab deleted successfully!";
            return RedirectToAction("ManageLabs");
        }

        #endregion ---------------- ManageLabs ----------------
    }
}
