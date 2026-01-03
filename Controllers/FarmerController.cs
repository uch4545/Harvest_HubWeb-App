using Data;
using Harvest_Hub.ViewModels;
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HarvestHub.WebApp.Controllers
{
    [Authorize(Roles = "Farmer")]
    public class FarmerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public FarmerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region ==================== LOGIN ====================

        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Login()
        {
            // If already authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // If already a Farmer, go to dashboard
                    if (await _userManager.IsInRoleAsync(user, "Farmer"))
                    {
                        return RedirectToAction("Dashboard");
                    }
                    
                    // If logged in as different role, sign out first
                    await _signInManager.SignOutAsync();
                    TempData["Info"] = "You were logged in as " + user.RoleType + ". Please login as Farmer.";
                }
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.Email, password, false, false);
                if (result.Succeeded)
                    return RedirectToAction("Dashboard");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        #endregion

        #region ==================== REGISTER ====================

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string cnic, string email, string password)
        {
            if (!ModelState.IsValid)
                return View();

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View();
            }

            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FullName = fullName,
                CNIC = cnic,
                RoleType = "Farmer"
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Farmer");
                await _signInManager.SignInAsync(user, false);

                var farmer = new Farmer
                {
                    FullName = fullName,
                    CNIC = cnic,
                    Email = email,
                    ApplicationUserId = user.Id
                };

                _context.Farmers.Add(farmer);
                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        #endregion

        #region ==================== DASHBOARD ====================

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var farmer = _context.Farmers.FirstOrDefault(f => f.ApplicationUserId == user.Id);

                if (farmer == null)
                    return RedirectToAction("Register");

                // Get unread notification count
                var unreadCount = await _context.Notifications
                    .Where(n => n.FarmerId == farmer.Id && !n.IsRead)
                    .CountAsync();
                    
                ViewBag.UnreadNotifications = unreadCount;

                // Quick Stats
                var totalCrops = await _context.Crops
                    .Where(c => c.FarmerId == farmer.Id)
                    .CountAsync();
                
                var activeListings = await _context.Crops
                    .Where(c => c.FarmerId == farmer.Id && c.Quantity > 0)
                    .CountAsync();
                
                var messages = await _context.Conversations
                    .Where(c => c.FarmerId == farmer.ApplicationUserId)
                    .CountAsync();
                
                var labReports = await _context.LabReports
                    .Where(r => r.FarmerId == farmer.Id)
                    .CountAsync();

                ViewBag.TotalCrops = totalCrops;
                ViewBag.ActiveListings = activeListings;
                ViewBag.Messages = messages;
                ViewBag.LabReports = labReports;

                return View(farmer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading dashboard. Please try again.";
                LogError("Dashboard", ex);
                return RedirectToAction("Login");
            }
        }

        #endregion

        #region ==================== CROPS CRUD ====================

        // CROP LIST
        public async Task<IActionResult> MyCrops()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);

                var crops = await _context.Crops
                                          .Where(c => c.FarmerId == farmer.Id)
                                          .Include(c => c.Images)
                                          .Include(c => c.Farmer)
                                          .ToListAsync();

                return View(crops);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading crops. Please try again.";
                LogError("MyCrops", ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public IActionResult AddCrop()
        {
            try
            {
                ViewBag.Labs = _context.Laboratories
                                       .Where(l => l.IsVerified)
                                       .ToList();
                return View(new Crop());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the form. Please try again.";
                LogError("AddCrop_GET", ex);
                return RedirectToAction("MyCrops");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCrop(Crop model, List<IFormFile> images, int laboratoryId, IFormFile reportFile)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);

                if (farmer == null)
                {
                    ModelState.AddModelError("", "Farmer record not found.");
                    return View(model);
                }

                model.FarmerId = farmer.Id;

                if (reportFile != null && reportFile.Length > 0 && laboratoryId > 0)
                {
                    var reportsFolder = Path.Combine("wwwroot", "reports");
                    if (!Directory.Exists(reportsFolder))
                        Directory.CreateDirectory(reportsFolder);

                    var reportPath = Path.Combine(reportsFolder, Guid.NewGuid() + Path.GetExtension(reportFile.FileName));
                    using (var stream = new FileStream(reportPath, FileMode.Create))
                    {
                        await reportFile.CopyToAsync(stream);
                    }

                    var report = new LabReport
                    {
                        FarmerId = farmer.Id,
                        LaboratoryId = laboratoryId,
                        ReportFilePath = "/reports/" + Path.GetFileName(reportPath),
                        SubmittedAt = DateTime.UtcNow
                    };

                    _context.LabReports.Add(report);
                    await _context.SaveChangesAsync();

                    model.ReportId = report.Id;
                }

                _context.Crops.Add(model);
                await _context.SaveChangesAsync();

                if (images != null && images.Count > 0)
                {
                    var imagesFolder = Path.Combine("wwwroot", "crop-images");
                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    foreach (var img in images)
                    {
                        if (img.Length > 0)
                        {
                            var imgPath = Path.Combine(imagesFolder, Guid.NewGuid() + Path.GetExtension(img.FileName));
                            using (var stream = new FileStream(imgPath, FileMode.Create))
                            {
                                await img.CopyToAsync(stream);
                            }

                            _context.CropImages.Add(new CropImage
                            {
                                CropId = model.Id,
                                ImageUrl = "/crop-images/" + Path.GetFileName(imgPath)
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Crop added successfully with images and report!";
                return RedirectToAction("MyCrops");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while adding crop. Please try again.";
                LogError("AddCrop_POST", ex);
                ViewBag.Labs = _context.Laboratories.Where(l => l.IsVerified).ToList();
                return View(model);
            }
        }



        // GET: Edit Crop
        public async Task<IActionResult> EditCrop(int id)
        {
            try
            {
                var crop = await _context.Crops
                                         .Include(c => c.Images)
                                         .Include(c => c.Report)
                                         .FirstOrDefaultAsync(c => c.Id == id);

                if (crop == null) return NotFound();

                ViewBag.Labs = await _context.Laboratories.Where(l => l.IsVerified).ToListAsync();
                return View(crop);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading crop for editing. Please try again.";
                LogError("EditCrop_GET", ex);
                return RedirectToAction("MyCrops");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCrop(
            Crop model,
            List<IFormFile> newImages,
            int laboratoryId,
            IFormFile newReportFile)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);

                if (farmer == null) return Unauthorized();

                var crop = await _context.Crops
                                         .Include(c => c.Images)
                                         .FirstOrDefaultAsync(c => c.Id == model.Id);

                if (crop == null) return NotFound();

                // Update basic fields
                crop.Name = model.Name;
                crop.Variety = model.Variety;
                crop.Quantity = model.Quantity;
                crop.Unit = model.Unit;
                crop.PricePerUnit = model.PricePerUnit;
                crop.Description = model.Description;
                crop.FarmerId = farmer.Id;

                // Replace report if uploaded
                if (newReportFile != null && newReportFile.Length > 0 && laboratoryId > 0)
                {
                    var reportsFolder = Path.Combine("wwwroot", "reports");
                    if (!Directory.Exists(reportsFolder))
                        Directory.CreateDirectory(reportsFolder);

                    var reportPath = Path.Combine(reportsFolder, Guid.NewGuid() + Path.GetExtension(newReportFile.FileName));
                    using (var stream = new FileStream(reportPath, FileMode.Create))
                    {
                        await newReportFile.CopyToAsync(stream);
                    }

                    var report = new LabReport
                    {
                        FarmerId = farmer.Id,
                        LaboratoryId = laboratoryId,
                        ReportFilePath = "/reports/" + Path.GetFileName(reportPath),
                        SubmittedAt = DateTime.UtcNow
                    };

                    _context.LabReports.Add(report);
                    await _context.SaveChangesAsync();

                    crop.ReportId = report.Id;
                }

                // Add new images
                if (newImages != null && newImages.Count > 0)
                {
                    var imagesFolder = Path.Combine("wwwroot", "crop-images");
                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    foreach (var img in newImages)
                    {
                        if (img.Length > 0)
                        {
                            var imgPath = Path.Combine(imagesFolder, Guid.NewGuid() + Path.GetExtension(img.FileName));
                            using (var stream = new FileStream(imgPath, FileMode.Create))
                            {
                                await img.CopyToAsync(stream);
                            }

                            _context.CropImages.Add(new CropImage
                            {
                                CropId = crop.Id,
                                ImageUrl = "/crop-images/" + Path.GetFileName(imgPath)
                            });
                        }
                    }
                }

                _context.Crops.Update(crop);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Crop updated successfully!";
                return RedirectToAction("MyCrops");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating crop. Please try again.";
                LogError("EditCrop_POST", ex);
                return RedirectToAction("MyCrops");
            }
        }


        // Delete Crop
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCrop(int id)
        {
            try
            {
                var crop = await _context.Crops.FindAsync(id);
                if (crop == null) return NotFound();

                _context.Crops.Remove(crop);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Crop deleted successfully!";
                return RedirectToAction("MyCrops");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting crop. Please try again.";
                LogError("DeleteCrop", ex);
                return RedirectToAction("MyCrops");
            }
        }
        //Details Crop

        public async Task<IActionResult> CropDetails(int id)
        {
            try
            {
                var crop = await _context.Crops
                                         .Include(c => c.Images)
                                         .Include(c => c.Farmer)
                                         .FirstOrDefaultAsync(c => c.Id == id);

                if (crop == null)
                    return NotFound();

                return View("~/Views/Crop/CropDetails.cshtml", crop);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading crop details. Please try again.";
                LogError("CropDetails", ex);
                return RedirectToAction("MyCrops");
            }
        }


        #endregion

        #region================== LIST REPORTS ==================
        public async Task<IActionResult> MyReports()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);

                var reports = await (from r in _context.LabReports
                                     join c in _context.Crops on r.Id equals c.ReportId into cropGroup
                                     from crop in cropGroup.DefaultIfEmpty()
                                     where r.FarmerId == farmer.Id
                                     select new ReportViewModel
                                     {
                                         Report = r,
                                         Crop = crop,
                                         Laboratory = r.Laboratory
                                     })
                                     .ToListAsync();

                return View(reports);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading reports. Please try again.";
                LogError("MyReports", ex);
                return RedirectToAction("Dashboard");
            }
        }
        #endregion

        #region ==================== NOTIFICATIONS ====================
        
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Notifications()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);

                if (farmer == null)
                    return RedirectToAction("Register");

                var notifications = await _context.Notifications
                    .Include(n => n.Order)
                        .ThenInclude(o => o.Crop)
                            .ThenInclude(c => c.Images)
                    .Include(n => n.Order.Buyer)
                    .Where(n => n.FarmerId == farmer.Id)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                ViewBag.FarmerName = farmer.FullName;
                return View(notifications);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading notifications. Please try again.";
                LogError("Notifications", ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int orderId, int notificationId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Notifications");
                }

                order.Status = "Accepted";
                
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order confirmed successfully!";
                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while confirming the order. Please try again.";
                LogError("ConfirmOrder", ex);
                return RedirectToAction("Notifications");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOrder(int orderId, int notificationId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Notifications");
                }

                order.Status = "Rejected";
                
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order rejected successfully.";
                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while rejecting the order. Please try again.";
                LogError("RejectOrder", ex);
                return RedirectToAction("Notifications");
            }
        }

        #endregion


        #region ==================== ERROR LOGGING ====================
        private void LogError(string actionName, Exception ex)
        {
            try
            {
                var error = new ErrorLog
                {
                    ControllerName = nameof(FarmerController),
                    ActionName = actionName,
                    UserId = User?.Identity?.Name,
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace
                };
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
            }
            catch { /* Fail silently to avoid recursive errors */ }
        }
        #endregion
    }
}
