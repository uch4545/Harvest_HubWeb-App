using Data;
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HarvestHub.WebApp.Data
{
    public static class SeedData
    {
        // ✅ Seed Admin Role + User
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
                    OtpCode = "000000",
                    OtpExpiry = DateTime.UtcNow.AddYears(10)
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        // ✅ Seed Market Rates Sample Data
        public static async Task SeedMarketRatesAsync(ApplicationDbContext context)
        {
            if (!context.MarketRates.Any())
            {
                context.MarketRates.AddRange(
                    new MarketRate { CropName = "Wheat", Rate = 4200, Date = DateTime.Now },
                    new MarketRate { CropName = "Rice", Rate = 5000, Date = DateTime.Now },
                    new MarketRate { CropName = "Sugarcane", Rate = 250, Date = DateTime.Now },
                    new MarketRate { CropName = "Cotton", Rate = 8700, Date = DateTime.Now },
                    new MarketRate { CropName = "Maize", Rate = 3100, Date = DateTime.Now }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
