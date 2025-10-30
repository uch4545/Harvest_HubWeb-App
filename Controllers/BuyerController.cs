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
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPost(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View("Login");
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View("Login");
            }

            return RedirectToAction("Dashboard");
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

        #endregion ==================== REGISTER ====================
        #region ==================== DASHBOARD ====================

        public async Task<IActionResult> Dashboard()
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

            // 👇 All crops list
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

            return View(model);
        }

        #endregion ==================== DASHBOARD ====================
        #region==================== CROPS DETAILS ====================
        public async Task <IActionResult> cropDetails(int id)
        {
            var crop = await _context.Crops
                             .Include(c => c.Images)
                             .Include(c => c.Farmer)
                             .FirstOrDefaultAsync(c => c.Id == id);
            if (crop == null)
                return NotFound();

            // Explicit full path for view
            return View("~/Views/Crop/CropDetails.cshtml", crop);

        }
        #endregion



    }
}
