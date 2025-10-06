using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Identity;

namespace HarvestHub.WebApp.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // ✅ Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // ✅ Default Admin User
            var adminEmail = "admin@harvesthub.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    RoleType = "Admin",
                    EmailConfirmed = true,
                    CNIC = "00000-0000000-0",
                    PhoneNumber = "0000000000",

                    // 🔹 Add these two lines
                    OtpCode = "000000",
                    OtpExpiry = DateTime.UtcNow.AddYears(10)
                };



                // Create with strong password
                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
