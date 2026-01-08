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
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    TempData["Error"] = "❌ Email and Password are required. / ای میل اور پاس ورڈ درکار ہیں۔";
                    return View();
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "❌ Invalid email or password. Please check your credentials. / غلط ای میل یا پاس ورڈ۔ براہ کرم اپنی تفصیلات چیک کریں۔";
                    return View();
                }

                var result = await _signInManager.PasswordSignInAsync(user.Email, password, false, false);
                if (!result.Succeeded)
                {
                    TempData["Error"] = "❌ Invalid email or password. Please check your credentials. / غلط ای میل یا پاس ورڈ۔ براہ کرم اپنی تفصیلات چیک کریں۔";
                    return View();
                }

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ An error occurred during login. Please try again. / لاگ ان کے دوران ایک خرابی پیش آئی۔ دوبارہ کوشش کریں۔";
                LogError("Login", ex);
                return View();
            }
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
                ViewBag.ProfileImagePath = farmer.ProfileImagePath; // For sidebar and dashboard

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
                    ViewBag.Labs = _context.Laboratories.Where(l => l.IsVerified).ToList();
                    return View(model);
                }

                // Validate lab report is required
                if (reportFile == null || reportFile.Length == 0 || laboratoryId <= 0)
                {
                    TempData["ErrorMessage"] = "Lab report is required. Please select a laboratory and upload a lab report file.";
                    ViewBag.Labs = _context.Laboratories.Where(l => l.IsVerified).ToList();
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
        [Authorize(Roles = "Farmer,Admin")]
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

        [Authorize(Roles = "Farmer,Admin")]
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

                // If Admin, get the farmer who owns this crop
                if (User.IsInRole("Admin") && farmer == null)
                {
                    var crop = await _context.Crops.Include(c => c.Farmer).FirstOrDefaultAsync(c => c.Id == model.Id);
                    if (crop != null)
                    {
                        farmer = crop.Farmer;
                    }
                }

                if (farmer == null) return Unauthorized();

                var existingCrop = await _context.Crops
                                         .Include(c => c.Images)
                                         .FirstOrDefaultAsync(c => c.Id == model.Id);

                if (existingCrop == null) return NotFound();

                // Update basic fields
                existingCrop.Name = model.Name;
                existingCrop.Variety = model.Variety;
                existingCrop.Quantity = model.Quantity;
                existingCrop.Unit = model.Unit;
                existingCrop.PricePerUnit = model.PricePerUnit;
                existingCrop.Description = model.Description;
                existingCrop.FarmerId = farmer.Id;

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

                    existingCrop.ReportId = report.Id;
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
                                CropId = existingCrop.Id,
                                ImageUrl = "/crop-images/" + Path.GetFileName(imgPath)
                            });
                        }
                    }
                }

                _context.Crops.Update(existingCrop);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Crop updated successfully!";
                
                // Redirect based on role
                if (User.IsInRole("Admin"))
                    return RedirectToAction("ManageProducts", "Admin");
                else
                    return RedirectToAction("MyCrops");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating crop. Please try again.";
                LogError("EditCrop_POST", ex);
                
                // Redirect based on role
                if (User.IsInRole("Admin"))
                    return RedirectToAction("ManageProducts", "Admin");
                else
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
                var crop = await _context.Crops
                    .Include(c => c.Images)
                    .FirstOrDefaultAsync(c => c.Id == id);
                    
                if (crop == null)
                {
                    TempData["ErrorMessage"] = "Crop not found.";
                    return RedirectToAction("MyCrops");
                }

                // Check if there are any ACTIVE/PENDING orders for this crop (not cancelled)
                var activeOrders = await _context.Orders
                    .Where(o => o.CropId == id && o.Status != "Cancelled")
                    .ToListAsync();
                
                if (activeOrders.Any())
                {
                    var orderCount = activeOrders.Count;
                    TempData["ErrorMessage"] = $"Cannot delete this crop. You have {orderCount} active order(s). Please cancel all orders first before deleting the crop. / آپ اس فصل کو حذف نہیں کر سکتے۔ آپ کے پاس {orderCount} فعال آرڈر ہیں۔ فصل حذف کرنے سے پہلے تمام آرڈرز منسوخ کریں۔";
                    return RedirectToAction("MyCrops");
                }

                // Delete all cancelled orders for this crop
                var cancelledOrders = await _context.Orders
                    .Where(o => o.CropId == id && o.Status == "Cancelled")
                    .ToListAsync();
                
                if (cancelledOrders.Any())
                {
                    var cancelledOrderIds = cancelledOrders.Select(o => o.Id).ToList();
                    
                    // Delete notifications related to these cancelled orders
                    var relatedNotifications = await _context.Notifications
                        .Where(n => n.OrderId.HasValue && cancelledOrderIds.Contains(n.OrderId.Value))
                        .ToListAsync();
                    
                    if (relatedNotifications.Any())
                    {
                        _context.Notifications.RemoveRange(relatedNotifications);
                    }
                    
                    _context.Orders.RemoveRange(cancelledOrders);
                }

                // Delete conversations related to this crop
                var conversations = await _context.Conversations
                    .Where(c => c.CropId == id)
                    .Include(c => c.Messages)
                    .ToListAsync();
                
                if (conversations.Any())
                {
                    // Delete messages first (cascade should handle this, but being explicit)
                    foreach (var conv in conversations)
                    {
                        if (conv.Messages != null && conv.Messages.Any())
                        {
                            _context.ChatMessages.RemoveRange(conv.Messages);
                        }
                    }
                    _context.Conversations.RemoveRange(conversations);
                }

                // Delete associated images
                if (crop.Images != null && crop.Images.Any())
                {
                    _context.CropImages.RemoveRange(crop.Images);
                }

                // Delete the crop
                _context.Crops.Remove(crop);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Crop deleted successfully!";
                return RedirectToAction("MyCrops");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting crop: {ex.Message}";
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

                // Get all unique lab reports for this farmer
                var labReports = await _context.LabReports
                    .Where(r => r.FarmerId == farmer.Id)
                    .Include(r => r.Laboratory)
                    .Distinct()
                    .ToListAsync();

                // Map to view model with associated crops
                var reports = labReports.Select(r => new ReportViewModel
                {
                    Report = r,
                    Crop = _context.Crops.FirstOrDefault(c => c.ReportId == r.Id),
                    Laboratory = r.Laboratory
                }).ToList();

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

                // Get all notifications - handle ones with and without orders separately
                var notifications = await _context.Notifications
                    .Where(n => n.FarmerId == farmer.Id)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                // Load related data for notifications that have orders
                foreach (var notification in notifications.Where(n => n.OrderId.HasValue))
                {
                    await _context.Entry(notification)
                        .Reference(n => n.Order)
                        .LoadAsync();

                    if (notification.Order != null)
                    {
                        await _context.Entry(notification.Order)
                            .Reference(o => o.Crop)
                            .LoadAsync();

                        await _context.Entry(notification.Order)
                            .Reference(o => o.Buyer)
                            .LoadAsync();

                        if (notification.Order.Crop != null)
                        {
                            await _context.Entry(notification.Order.Crop)
                                .Collection(c => c.Images)
                                .LoadAsync();
                        }
                    }
                }

                ViewBag.FarmerName = farmer.FullName;
                return View(notifications);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while loading notifications: {ex.Message}";
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrderByFarmer(int orderId, int notificationId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);

                if (farmer == null)
                    return RedirectToAction("Register");

                var order = await _context.Orders
                    .Include(o => o.Crop)
                    .Include(o => o.Buyer)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.Crop.FarmerId == farmer.Id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Notifications");
                }

                // Check if order can be cancelled (only Accepted orders)
                if (order.Status != "Accepted")
                {
                    TempData["ErrorMessage"] = "This order cannot be cancelled. Only accepted orders can be cancelled.";
                    return RedirectToAction("Notifications");
                }

                // Update order status to Cancelled
                order.Status = "Cancelled";

                // Mark notification as read
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                // Note: Buyer notifications will be handled separately via notification checks
                TempData["SuccessMessage"] = "Order cancelled successfully!";
                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while cancelling the order. Please try again.";
                LogError("CancelOrderByFarmer", ex);
                return RedirectToAction("Notifications");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotificationOrder(int notificationId, int? orderId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);

                if (farmer == null)
                    return RedirectToAction("Register");

                // Delete the order if specified (must delete all notifications first)
                if (orderId.HasValue)
                {
                    var order = await _context.Orders
                        .Include(o => o.Crop)
                        .FirstOrDefaultAsync(o => o.Id == orderId.Value && o.Crop.FarmerId == farmer.Id);

                    if (order != null)
                    {
                        // Delete ALL notifications related to this order FIRST (foreign key constraint)
                        var allRelatedNotifications = await _context.Notifications
                            .Where(n => n.OrderId == orderId.Value)
                            .ToListAsync();

                        if (allRelatedNotifications.Any())
                        {
                            _context.Notifications.RemoveRange(allRelatedNotifications);
                            await _context.SaveChangesAsync(); // Save notification deletions first
                        }

                        // Now delete the order
                        _context.Orders.Remove(order);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Just delete the single notification
                    var notification = await _context.Notifications
                        .FirstOrDefaultAsync(n => n.Id == notificationId && n.FarmerId == farmer.Id);

                    if (notification != null)
                    {
                        _context.Notifications.Remove(notification);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Notification/Order removed successfully!";
                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting: {ex.Message}";
                LogError("DeleteNotificationOrder", ex);
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
