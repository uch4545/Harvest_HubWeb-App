using Data;
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HarvestHub.WebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.RoleType == "Farmer")
            {
                var farmer = await _context.Farmers
                    .FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
                return View("FarmerProfile", farmer);
            }
            else if (user.RoleType == "Buyer")
            {
                var buyer = await _context.Buyers
                    .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);
                return View("BuyerProfile", buyer);
            }
            else if (user.RoleType == "Admin")
            {
                return View("AdminProfile", user);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.RoleType == "Farmer")
            {
                var farmer = await _context.Farmers
                    .FirstOrDefaultAsync(f => f.ApplicationUserId == user.Id);
                return View("EditFarmerProfile", farmer);
            }
            else if (user.RoleType == "Buyer")
            {
                var buyer = await _context.Buyers
                    .FirstOrDefaultAsync(b => b.ApplicationUserId == user.Id);
                return View("EditBuyerProfile", buyer);
            }

            return RedirectToAction("Index");
        }

        // POST: /Profile/UpdateFarmer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFarmer(Farmer model, IFormFile? profileImage)
        {
            try
            {
                var farmer = await _context.Farmers.FindAsync(model.Id);
                if (farmer == null) return NotFound();

                // Update basic info
                farmer.FullName = model.FullName;
                farmer.Email = model.Email;
                farmer.PhoneNumber = model.PhoneNumber;

                // Handle profile image upload
                if (profileImage != null && profileImage.Length > 0)
                {
                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(farmer.ProfileImagePath))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, farmer.ProfileImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{farmer.ApplicationUserId}_{DateTime.Now.Ticks}{Path.GetExtension(profileImage.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(fileStream);
                    }

                    farmer.ProfileImagePath = $"/uploads/profiles/{uniqueFileName}";
                }

                _context.Update(farmer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating profile. Please try again.";
                Console.WriteLine($"Profile update error: {ex.Message}");
                return View("EditFarmerProfile", model);
            }
        }

        // POST: /Profile/UpdateBuyer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBuyer(Buyer model, IFormFile? profileImage)
        {
            try
            {
                var buyer = await _context.Buyers.FindAsync(model.Id);
                if (buyer == null) return NotFound();

                // Update basic info
                buyer.FullName = model.FullName;
                buyer.Email = model.Email;
                buyer.PhoneNumber = model.PhoneNumber;

                // Handle profile image upload
                if (profileImage != null && profileImage.Length > 0)
                {
                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(buyer.ProfileImagePath))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, buyer.ProfileImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{buyer.ApplicationUserId}_{DateTime.Now.Ticks}{Path.GetExtension(profileImage.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(fileStream);
                    }

                    buyer.ProfileImagePath = $"/uploads/profiles/{uniqueFileName}";
                }

                _context.Update(buyer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating profile. Please try again.";
                Console.WriteLine($"Profile update error: {ex.Message}");
                return View("EditBuyerProfile", model);
            }
        }
    }
}
