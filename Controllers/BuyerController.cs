using Data;
using HarvestHub.WebApp.Models;
using HarvestHub.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HarvestHub.Controllers
{
    [Authorize(Roles = "Buyer")]
    public class BuyerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public BuyerController(
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
                    // If already a Buyer, go to dashboard
                    if (await _userManager.IsInRoleAsync(user, "Buyer"))
                    {
                        return RedirectToAction("Dashboard");
                    }
                    
                    // If logged in as different role, sign out first
                    await _signInManager.SignOutAsync();
                    TempData["Info"] = "You were logged in as " + user.RoleType + ". Please login as Buyer.";
                }
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPost(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    TempData["Error"] = "❌ Email and Password are required. / ای میل اور پاس ورڈ درکار ہیں۔";
                    return View("Login");
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "❌ Invalid email or password. Please check your credentials. / غلط ای میل یا پاس ورڈ۔ براہ کرم اپنی تفصیلات چیک کریں۔";
                    return View("Login");
                }

                var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

                if (!result.Succeeded)
                {
                    TempData["Error"] = "❌ Invalid email or password. Please check your credentials. / غلط ای میل یا پاس ورڈ۔ براہ کرم اپنی تفصیلات چیک کریں۔";
                    return View("Login");
                }

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ An error occurred during login. Please try again. / لاگ ان کے دوران ایک خرابی پیش آئی۔ دوبارہ کوشش کریں۔";
                LogError("LoginPost", ex);
                return View("Login");
            }
        }

        #endregion ==================== LOGIN ====================

        #region ==================== REGISTER ====================

        [AllowAnonymous]
        public IActionResult Register()
        {
            return RedirectToAction("BuyerRegister", "Account");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterBuyer(BuyerRegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    FullName = model.FullName,
                    CNIC = model.CNIC,
                    RoleType = "Buyer"
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Buyer");
                    await _signInManager.SignInAsync(user, false);

                    var buyer = new Buyer
                    {
                        FullName = model.FullName,
                        CNIC = model.CNIC,
                        Email = model.Email,
                        PhoneNumber = user.PhoneNumber,
                        ApplicationUserId = user.Id
                    };

                    _context.Buyers.Add(buyer);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Dashboard");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred during registration. Please try again.";
                LogError("RegisterBuyer", ex);
                return View(model);
            }
        }

        #endregion ==================== REGISTER ====================
        #region ==================== DASHBOARD ====================

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

                if (buyer == null)
                    return RedirectToAction("Register");

                var myOrders = await _context.Orders
                                             .Where(o => o.BuyerId == buyer.Id)
                                             .OrderByDescending(o => o.OrderDate)
                                             .Take(5)
                                             .ToListAsync();

                var crops = await _context.Crops
                                          .Include(c => c.Images)
                                          .Include(c => c.Farmer)
                                          .ToListAsync();

                var model = new BuyerDashboardViewModel
                {
                    Buyer = buyer,
                    Crops = crops,
                    RecentOrders = myOrders
                };
                
                ViewBag.ProfileImagePath = buyer.ProfileImagePath; // For sidebar and dashboard

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading dashboard. Please try again.";
                LogError("Dashboard", ex);
                return RedirectToAction("Login");
            }
        }

        #endregion ==================== DASHBOARD ====================
        #region==================== CROPS DETAILS ====================
        public async Task<IActionResult> cropDetails(int id)
        {
            try
            {
                var crop = await _context.Crops
                                 .Include(c => c.Images)
                                 .Include(c => c.Farmer)
                                 .Include(c => c.Report)
                                     .ThenInclude(r => r.Laboratory)
                                 .FirstOrDefaultAsync(c => c.Id == id);
                if (crop == null)
                    return NotFound();

                return View("~/Views/Crop/CropDetails.cshtml", crop);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading crop details. Please try again.";
                LogError("cropDetails", ex);
                return RedirectToAction("Dashboard");
            }
        }
        #endregion
        
        #region ==================== ORDERS ====================
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int cropId, decimal quantity, string unit)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

                if (buyer == null)
                    return RedirectToAction("Register");

                // Get crop details
                var crop = await _context.Crops
                    .Include(c => c.Farmer)
                    .FirstOrDefaultAsync(c => c.Id == cropId);

                if (crop == null)
                {
                    TempData["ErrorMessage"] = "Crop not found.";
                    return RedirectToAction("Dashboard");
                }

                // Validate quantity
                if (quantity <= 0)
                {
                    TempData["ErrorMessage"] = "Please enter a valid quantity.";
                    return RedirectToAction("Dashboard");
                }

                // Calculate total price
                decimal totalPrice = quantity * crop.PricePerUnit;

                // Create order
                var order = new Order
                {
                    BuyerId = buyer.Id,
                    CropId = cropId,
                    Quantity = quantity,
                    TotalPrice = totalPrice,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create notification for farmer
                var notification = new Notification
                {
                    FarmerId = crop.FarmerId,
                    OrderId = order.Id,
                    BuyerName = buyer.FullName,
                    CropName = crop.Name,
                    Quantity = quantity,
                    TotalPrice = totalPrice,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Message = $"New order from {buyer.FullName} for {quantity} {unit} of {crop.Name}"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Order placed successfully! Total: Rs. {totalPrice:N0}";
                return RedirectToAction("MyOrders");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while placing the order. Please try again.";
                LogError("PlaceOrder", ex);
                return RedirectToAction("Dashboard");
            }
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> MyOrders()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

                if (buyer == null)
                    return RedirectToAction("Register");

                var orders = await _context.Orders
                    .Include(o => o.Crop)
                        .ThenInclude(c => c.Farmer)
                    .Include(o => o.Crop.Images)
                    .Where(o => o.BuyerId == buyer.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                ViewBag.BuyerName = buyer.FullName;
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading your orders. Please try again.";
                LogError("MyOrders", ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

                if (buyer == null)
                    return RedirectToAction("Register");

                var order = await _context.Orders
                    .Include(o => o.Crop)
                        .ThenInclude(c => c.Farmer)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == buyer.Id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("MyOrders");
                }

                // Check if order can be cancelled
                if (order.Status != "Pending" && order.Status != "Accepted")
                {
                    TempData["ErrorMessage"] = "This order cannot be cancelled.";
                    return RedirectToAction("MyOrders");
                }

                // Update order status to Cancelled
                order.Status = "Cancelled";
                await _context.SaveChangesAsync();

                // Send notification to farmer
                var notification = new Notification
                {
                    FarmerId = order.Crop.FarmerId,
                    OrderId = order.Id,
                    NotificationType = "OrderCancelled",
                    BuyerName = buyer.FullName,
                    CropName = order.Crop.Name,
                    Quantity = order.Quantity,
                    TotalPrice = order.TotalPrice,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Message = $"Buyer {buyer.FullName} cancelled the order for {order.Quantity} kg of {order.Crop.Name}. / خریدار نے آرڈر منسوخ کر دیا۔"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order cancelled successfully!";
                return RedirectToAction("MyOrders");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while cancelling the order. Please try again.";
                LogError("CancelOrder", ex);
                return RedirectToAction("MyOrders");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var buyer = await _context.Buyers.FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);

                if (buyer == null)
                    return RedirectToAction("Register");

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == buyer.Id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("MyOrders");
                }

                // Delete associated notifications FIRST (due to foreign key constraint)
                var notifications = await _context.Notifications
                    .Where(n => n.OrderId == orderId)
                    .ToListAsync();

                if (notifications.Any())
                {
                    _context.Notifications.RemoveRange(notifications);
                    await _context.SaveChangesAsync(); // Save notification deletions first
                }

                // Now delete the order
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order removed successfully!";
                return RedirectToAction("MyOrders");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the order: {ex.Message}";
                LogError("DeleteOrder", ex);
                return RedirectToAction("MyOrders");
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
                    ControllerName = nameof(BuyerController),
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
