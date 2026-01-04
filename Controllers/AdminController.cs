using Data; // your DbContext
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region ---------------- Dashboard ----------------
        
        public async Task<IActionResult> Dashboard()
        {
            try
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
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading dashboard. Please try again.";
                LogError("Dashboard", ex);
                return View();
            }
        }

        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var farmers = await _context.Farmers.ToListAsync();
                var buyers = await _context.Buyers.ToListAsync();
                ViewBag.Farmers = farmers;
                ViewBag.Buyers = buyers;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading users. Please try again.";
                LogError("ManageUsers", ex);
                return RedirectToAction("Dashboard");
            }
        }

        public async Task<IActionResult> ManageProducts()
        {
            try
            {
                var crops = await _context.Crops.Include(c => c.Farmer).ToListAsync();
                return View(crops);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading products. Please try again.";
                LogError("ManageProducts", ex);
                return RedirectToAction("Dashboard");
            }
        }

        public async Task<IActionResult> VerifyDocuments()
        {
            try
            {
                var docs = await _context.VerificationDocument
                              .Include(v => v.User)
                              .OrderByDescending(v => v.SubmittedAt)
                              .ToListAsync();

                return View(docs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading documents. Please try again.";
                LogError("VerifyDocuments", ex);
                return RedirectToAction("Dashboard");
            }
        }

        #endregion ---------------- Dashboard ----------------

        #region ---------------- Manage Products/Crops ----------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCrop(int id)
        {
            try
            {
                var crop = await _context.Crops
                    .Include(c => c.Images)
                    .Include(c => c.Farmer)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (crop == null) 
                {
                    TempData["ErrorMessage"] = "Crop not found.";
                    return RedirectToAction("ManageProducts");
                }

                // Store farmer info and crop name before deletion
                var farmerId = crop.FarmerId;
                var cropName = crop.Name;

                // Delete associated images
                if (crop.Images != null && crop.Images.Any())
                {
                    _context.CropImages.RemoveRange(crop.Images);
                }

                _context.Crops.Remove(crop);
                await _context.SaveChangesAsync();

                // Send notification to farmer
                var notification = new Notification
                {
                    FarmerId = farmerId,
                    NotificationType = "CropDeleted",
                    CropName = cropName,
                    Message = $"Your crop '{cropName}' has been deleted by the Administrator. / آپ کی فصل '{cropName}' کو منتظم نے حذف کر دیا ہے۔",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Crop '{cropName}' deleted successfully!";
                return RedirectToAction("ManageProducts");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting crop. Please try again.";
                LogError("DeleteCrop", ex);
                return RedirectToAction("ManageProducts");
            }
        }

        #endregion ---------------- Manage Products/Crops ----------------

        #region ---------------- ManageLabs ----------------

        public async Task<IActionResult> ManageLabs()
        {
            try
            {
                var labs = await _context.Laboratories.ToListAsync();
                return View(labs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading labs. Please try again.";
                LogError("ManageLabs", ex);
                return RedirectToAction("Dashboard");
            }
        }

        public IActionResult CreateLab()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLab(Laboratory lab)
        {
            try
            {
                if (!ModelState.IsValid) return View(lab);

                _context.Laboratories.Add(lab);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lab added successfully!";
                return RedirectToAction("ManageLabs");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating lab. Please try again.";
                LogError("CreateLab", ex);
                return View(lab);
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyLab(int id)
        {
            try
            {
                var lab = await _context.Laboratories.FindAsync(id);
                if (lab != null)
                {
                    lab.IsVerified = true;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Lab verified successfully!";
                }
                return RedirectToAction("ManageLabs");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while verifying lab. Please try again.";
                LogError("VerifyLab", ex);
                return RedirectToAction("ManageLabs");
            }
        }

        public async Task<IActionResult> EditLab(int id)
        {
            try
            {
                var lab = await _context.Laboratories.FindAsync(id);
                if (lab == null) return NotFound();
                return View(lab);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading lab. Please try again.";
                LogError("EditLab_GET", ex);
                return RedirectToAction("ManageLabs");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLab(Laboratory lab)
        {
            try
            {
                if (!ModelState.IsValid) return View(lab);

                _context.Laboratories.Update(lab);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lab updated successfully!";
                return RedirectToAction("ManageLabs");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating lab. Please try again.";
                LogError("EditLab_POST", ex);
                return View(lab);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLab(int id)
        {
            try
            {
                var lab = await _context.Laboratories.FindAsync(id);
                if (lab == null) return NotFound();

                _context.Laboratories.Remove(lab);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Lab deleted successfully!";
                return RedirectToAction("ManageLabs");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting lab. Please try again.";
                LogError("DeleteLab", ex);
                return RedirectToAction("ManageLabs");
            }
        }

        #endregion ---------------- ManageLabs ----------------

        #region ---------------- Error Logging ----------------
        private void LogError(string actionName, Exception ex)
        {
            try
            {
                var error = new ErrorLog
                {
                    ControllerName = nameof(AdminController),
                    ActionName = actionName,
                    UserId = User?.Identity?.Name,
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace
                };
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
            }
            catch {  }
        }
        #endregion
    }
}
