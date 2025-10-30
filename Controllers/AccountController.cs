using Data;
using HarvestHub.WebApp.Models;
using HarvestHub.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace HarvestHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context;
        }

        #region ---------------- Buyer Registration ----------------

        [HttpGet]
        public IActionResult BuyerRegister()
        {
            return View(new BuyerRegisterViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyerRegister(BuyerRegisterViewModel model)
        {
            model.RoleType = "Buyer";
            return await HandleBuyerRegistration(model);
        }
        private async Task<IActionResult> HandleBuyerRegistration(BuyerRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("BuyerRegister", model);

            // ✅ Password strength validation
            if (!Regex.IsMatch(model.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,}$"))
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters and include uppercase, lowercase, digit, and special character.");
                return View("BuyerRegister", model);
            }

            // ✅ Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View("BuyerRegister", model);
            }

            // ✅ Create new user
            var user = new ApplicationUser
            {
                Email = model.Email,
                CNIC = model.CNIC,
                RoleType = model.RoleType,
                UserName = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            // ✅ Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // ✅ Ensure role exists
                if (!await _roleManager.RoleExistsAsync(model.RoleType))
                    await _roleManager.CreateAsync(new IdentityRole(model.RoleType));

                // ✅ Assign role to user
                await _userManager.AddToRoleAsync(user, model.RoleType);

                // ✅ Store OTP in TempData
                TempData["Otp"] = otp;
                TempData["Email"] = model.Email;

                // ✅ Send OTP Email
                try
                {
                    await _emailSender.SendEmailAsync(
                        user.Email,
                        "Harvest Hub OTP Verification",
                        $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px; border: 1px solid #ddd; max-width: 500px; margin: auto;'>
                            <h2 style='color: #2e7d32; text-align: center;'>Harvest Hub</h2>
                            <p style='font-size: 16px; color: #333;'>Hello,</p>
                            <p style='font-size: 16px; color: #333;'>
                                Your One-Time Password (OTP) for verification is:
                            </p>
                            <p style='font-size: 24px; font-weight: bold; color: #2e7d32; text-align: center; background: #e8f5e9; padding: 10px; border-radius: 5px;'>
                                {otp}
                            </p>
                            <p style='font-size: 14px; color: #777;'>
                                ⚠️ This OTP is valid for the next 5 minutes. Please do not share it with anyone.
                            </p>
                            <p style='font-size: 14px; color: #333; margin-top: 20px;'>
                                Regards,<br/>
                                <strong>Harvest Hub Team</strong>
                            </p>
                        </div>
                        "
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Email sending failed: " + ex.Message);
                    return View("BuyerRegister", model);
                }

                return RedirectToAction("VerifyOtp");
            }

            // Identity Errors
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View("BuyerRegister", model);
        }

        #endregion

        #region ---------------- Farmer Registration ----------------

        [HttpGet]
        public IActionResult FarmerRegister()
        {
            return View(new FarmerRegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FarmerRegister(FarmerRegisterViewModel model)
        {
            model.RoleType = "Farmer";
            return await HandleFarmerRegistration(model);
        }

        private async Task<IActionResult> HandleFarmerRegistration(FarmerRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("FarmerRegister", model);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View("FarmerRegister", model);
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                CNIC = model.CNIC,
                RoleType = model.RoleType,
                UserName = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            // ✅ Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.RoleType))
                    await _roleManager.CreateAsync(new IdentityRole(model.RoleType));

                await _userManager.AddToRoleAsync(user, model.RoleType);

                TempData["Otp"] = otp;
                TempData["Email"] = model.Email;

                try
                {
                    await _emailSender.SendEmailAsync(
                        user.Email,
                        "Harvest Hub OTP Verification",
                        $@"
                        <div style='font-family:Arial,sans-serif; max-width:600px; margin:auto; padding:20px; border:1px solid #ddd; border-radius:10px; background-color:#f9f9f9;'>
                            <h2 style='color:#2e7d32; text-align:center;'>Harvest Hub</h2>
                            <p style='font-size:16px; color:#333;'>Dear User,</p>
                            <p style='font-size:16px; color:#333;'>
                                Your One-Time Password (OTP) for verification is:
                            </p>
                            <p style='font-size:24px; font-weight:bold; text-align:center; color:#2e7d32; background:#e8f5e9; padding:10px; border-radius:5px;'>
                                {otp}
                            </p>
                            <p style='font-size:14px; color:#777;'>
                                This OTP will expire in 5 minutes. Please do not share it with anyone.
                            </p>
                            <br />
                            <p style='font-size:14px; color:#333;'>Best Regards,<br/>Harvest Hub Team</p>
                        </div>
                        "
                    );
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Email sending failed: " + ex.Message);
                    return View("FarmerRegister", model);
                }

                return RedirectToAction("VerifyOtp");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View("FarmerRegister", model);
        }

        #endregion

        #region ---------------- OTP Verification ----------------

        // GET
        [HttpGet("Account/VerifyOtp")]
        public IActionResult VerifyOtp()
        {
            return View(new VerifyOtpViewModel());
        }

        // POST
        [HttpPost("Account/VerifyOtp")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            var originalOtp = TempData.Peek("Otp")?.ToString();
            var email = TempData.Peek("Email")?.ToString();

            if (originalOtp == null || email == null)
            {
                ModelState.AddModelError("", "OTP expired. Please register again.");
                return View(new VerifyOtpViewModel()); // 🔹 Farmer case me bhi ek generic Register action bana sakte ho
            }

            if (model.OtpCode == originalOtp)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    // Confirm email
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    // Auto sign in
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (user.RoleType == "Buyer")
                    {
                        bool alreadyExists = await _context.Buyers.AnyAsync(b => b.ApplicationUserId == user.Id);
                        if (!alreadyExists)
                        {
                            var buyer = new Buyer
                            {
                                ApplicationUserId = user.Id,
                                FullName = user.FullName,
                                Email = user.Email,
                                CNIC = user.CNIC,
                                PasswordHash = user.PasswordHash,
                                PhoneNumber = user.PhoneNumber
                            };

                            _context.Buyers.Add(buyer);
                            await _context.SaveChangesAsync();
                        }

                        return RedirectToAction("Dashboard", "Buyer");
                    }

                    else if (user.RoleType == "Farmer")
                    {
                        bool alreadyExists = await _context.Farmers.AnyAsync(f => f.ApplicationUserId == user.Id);
                        if (!alreadyExists)
                        {
                            var farmer = new Farmer
                            {
                                ApplicationUserId = user.Id,
                                FullName = user.FullName,
                                Email = user.Email!,
                                CNIC = user.CNIC,
                                PasswordHash = user.PasswordHash,
                                PhoneNumber = user.PhoneNumber
                            };

                            _context.Farmers.Add(farmer);
                            await _context.SaveChangesAsync();
                        }

                        return RedirectToAction("Dashboard", "Farmer");
                    }
                }
            }

            // If OTP incorrect
            ModelState.AddModelError("", "Incorrect OTP. Please try again.");
            TempData["Otp"] = originalOtp;
            TempData["Email"] = email;
            return View(new VerifyOtpViewModel());
        }


        #endregion

        #region ---------------- Resend OTP ----------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is missing. Please register again.");
                return RedirectToAction("BuyerRegister");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return RedirectToAction("BuyerRegister");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            TempData["Otp"] = otp;
            TempData["Email"] = email;

            try
            {
                await _emailSender.SendEmailAsync(
                    email,
                    "Harvest Hub OTP Resend",
                    $"<p>Your new OTP is: <strong>{otp}</strong></p>"
                );
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to resend OTP: " + ex.Message);
                return RedirectToAction("VerifyOtp");
            }

            TempData["SuccessMessage"] = "OTP resent successfully.";
            return RedirectToAction("VerifyOtp");
        }

        #endregion

        #region ---------------- Logout ----------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Buyer");
        }

        #endregion

        #region ---------------- Forgot Password ----------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View();
            }

            var user = _userManager.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Security: Don't reveal user existence
                TempData["SuccessMessage"] = "If this email exists, a reset link has been sent.";
                return RedirectToAction("ForgotPassword");
            }

            // Generate reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action(
                "ResetPassword", "Account",
                new { token, email = user.Email },
                protocol: HttpContext.Request.Scheme);

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Harvest Hub - Password Reset",
                    $@"
                    <div style='font-family:Arial,sans-serif; max-width:600px; margin:auto; padding:20px; border:1px solid #ddd; border-radius:10px; background-color:#f9f9f9;'>
                        <h2 style='color:#2e7d32; text-align:center;'>Harvest Hub</h2>
                        <p style='font-size:16px; color:#333;'>Hello,</p>
                        <p style='font-size:16px; color:#333;'>
                            We received a request to reset your password. Click the link below to set a new password:
                        </p>
                        <p style='text-align:center; margin:20px 0;'>
                            <a href='{resetLink}' style='background:#2e7d32; color:#fff; padding:10px 20px; text-decoration:none; border-radius:5px;'>Reset Password</a>
                        </p>
                        <p style='font-size:14px; color:#777;'>
                            If you did not request this, please ignore this email.
                        </p>
                        <br />
                        <p style='font-size:14px; color:#333;'>Best Regards,<br/>Harvest Hub Team</p>
                    </div>
                    "
                );
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to send reset email: " + ex.Message);
                return View();
            }

            TempData["SuccessMessage"] = "Password reset link sent to your email.";
            return RedirectToAction("ForgotPassword");
        }

        #endregion

        #region ---------------- Reset Password ----------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["SuccessMessage"] = "Password reset successful.";
                return RedirectToAction("Login", "Buyer");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password reset successful. You can now log in.";
                return RedirectToAction("Login", user.RoleType == "Farmer" ? "Farmer" : "Buyer");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        #endregion

        #region ---------------- VerificationDocument ----------------
        [HttpGet]
        public IActionResult UploadVerification()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadVerification(IFormFile file, string documentType)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var path = Path.Combine("wwwroot/uploads/verifications", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using (var stream = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(stream);

            var doc = new VerificationDocument
            {
                UserId = userId,
                DocumentType = documentType,
                FilePath = $"/uploads/verifications/{fileName}",
                Status = WebApp.Models.VerificationStatus.Pending
            };

            _context.VerificationDocument.Add(doc);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Verification document submitted successfully.";
            return RedirectToAction("VerificationStatus");
        }

        [HttpGet]
        public async Task<IActionResult> VerificationStatus()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var docs = await _context.VerificationDocument
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.SubmittedAt)
                .ToListAsync();

            return View(docs);
        }



        #endregion ---------------- VerificationDocument ----------------

        #region ---------------- Admin Login ----------------

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AdminLogin()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.RoleType != "Admin")
            {
                ModelState.AddModelError("", "Invalid credentials or not authorized as Admin.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            ModelState.AddModelError("", "Login failed. Please try again.");
            return View(model);
        }

        #endregion

        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl)
        {
            ViewBag.ErrorMessage = "You are not authorized to access this page.";
            return View("Error");
        }

    }
}