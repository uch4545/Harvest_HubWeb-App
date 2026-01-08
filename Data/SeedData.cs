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
            // Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Default Admin User
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
                    new MarketRate { CropName = "Wheat / گندم", CropNameUrdu = "گندم", CurrentRate = 4200, Unit = "40 Kg", LastUpdated = DateTime.UtcNow },
                    new MarketRate { CropName = "Rice Basmati / باسمتی", CropNameUrdu = "باسمتی", CurrentRate = 9500, Unit = "40 Kg", LastUpdated = DateTime.UtcNow },
                    new MarketRate { CropName = "Sugarcane / گنا", CropNameUrdu = "گنا", CurrentRate = 320, Unit = "40 Kg", LastUpdated = DateTime.UtcNow },
                    new MarketRate { CropName = "Cotton / کپاس", CropNameUrdu = "کپاس", CurrentRate = 8700, Unit = "40 Kg", LastUpdated = DateTime.UtcNow },
                    new MarketRate { CropName = "Maize / مکئی", CropNameUrdu = "مکئی", CurrentRate = 3100, Unit = "40 Kg", LastUpdated = DateTime.UtcNow },
                    new MarketRate { CropName = "Potato / آلو", CropNameUrdu = "آلو", CurrentRate = 100, Unit = "Kg", LastUpdated = DateTime.UtcNow },
                    new MarketRate { CropName = "Onion / پیاز", CropNameUrdu = "پیاز", CurrentRate = 150, Unit = "Kg", LastUpdated = DateTime.UtcNow },
                    new MarketRate { CropName = "Tomato / ٹماٹر", CropNameUrdu = "ٹماٹر", CurrentRate = 180, Unit = "Kg", LastUpdated = DateTime.UtcNow }
                );

                await context.SaveChangesAsync();
            }
        }

        // ✅ Seed Government Schemes for Farmers
        public static async Task SeedGovernmentSchemesAsync(ApplicationDbContext context)
        {
            if (!context.GovernmentSchemes.Any())
            {
                context.GovernmentSchemes.AddRange(
                    new GovernmentScheme
                    {
                        Title = "PM Kisan Samman Nidhi",
                        TitleUrdu = "وزیر اعظم کسان سمان نیدھی",
                        Description = "Direct income support of Rs. 6,000 per year to small and marginal farmers in three equal installments. This scheme aims to supplement the financial needs of farmers in procuring various inputs to ensure proper crop health and appropriate yields.",
                        DescriptionUrdu = "چھوٹے اور معمولی کسانوں کو براہ راست آمدنی کی حمایت سالانہ 6,000 روپے تین مساوی قسطوں میں۔",
                        Category = "Subsidy",
                        Status = "Active",
                        IsFeatured = true,
                        Eligibility = "All landholding farmer families with cultivable land. Must have valid CNIC and bank account.",
                        Benefits = "Rs. 6,000 annual direct cash transfer in 3 installments of Rs. 2,000 each.",
                        HowToApply = "Visit nearest NADRA office or apply online through PM Kisan portal with CNIC and land documents.",
                        ContactInfo = "Helpline: 0800-00786",
                        OfficialLink = "https://www.pakistan.gov.pk",
                        StartDate = new DateTime(2019, 2, 1),
                        DisplayOrder = 1
                    },
                    new GovernmentScheme
                    {
                        Title = "Kissan Package 2024",
                        TitleUrdu = "کسان پیکج 2024",
                        Description = "Comprehensive relief package for farmers including subsidized fertilizers, reduced electricity rates for tube wells, and interest-free loans for small farmers.",
                        DescriptionUrdu = "کسانوں کے لیے جامع ریلیف پیکج جس میں سبسڈی والی کھادیں، ٹیوب ویلز کے لیے کم بجلی کی شرحیں، اور چھوٹے کسانوں کے لیے بلا سود قرضے شامل ہیں۔",
                        Category = "Subsidy",
                        Status = "Active",
                        IsFeatured = true,
                        Eligibility = "All registered farmers with valid land ownership documents.",
                        Benefits = "50% subsidy on DAP fertilizer, reduced electricity tariff, interest-free loans up to Rs. 150,000.",
                        HowToApply = "Register at local agriculture office with CNIC, land documents, and passbook.",
                        ContactInfo = "Agriculture Helpline: 0800-12345",
                        StartDate = new DateTime(2024, 1, 1),
                        DisplayOrder = 2
                    },
                    new GovernmentScheme
                    {
                        Title = "Zarai Taraqiati Bank Loans",
                        TitleUrdu = "زرعی ترقیاتی بینک قرضے",
                        Description = "Agricultural development loans for farmers at subsidized interest rates. Loans available for crop production, farm mechanization, livestock, and land development.",
                        DescriptionUrdu = "کسانوں کے لیے سبسڈی والی شرحوں پر زرعی ترقیاتی قرضے۔",
                        Category = "Loan",
                        Status = "Active",
                        Eligibility = "Farmers with land ownership or lease agreement. Good credit history preferred.",
                        Benefits = "Loans from Rs. 50,000 to Rs. 5,000,000. Interest rates as low as 8%.",
                        HowToApply = "Visit nearest ZTBL branch with land documents, CNIC, and application form.",
                        ContactInfo = "ZTBL Helpline: 051-9252670",
                        OfficialLink = "https://www.ztbl.com.pk",
                        StartDate = new DateTime(2000, 1, 1),
                        DisplayOrder = 3
                    },
                    new GovernmentScheme
                    {
                        Title = "Crop Insurance Scheme",
                        TitleUrdu = "فصل بیمہ اسکیم",
                        Description = "Government-backed crop insurance to protect farmers against natural calamities, pest attacks, and crop failures. Covers wheat, cotton, rice, sugarcane, and maize.",
                        DescriptionUrdu = "قدرتی آفات، کیڑوں کے حملوں اور فصلوں کی ناکامی سے کسانوں کی حفاظت کے لیے حکومت کی طرف سے فصل بیمہ۔",
                        Category = "Insurance",
                        Status = "Active",
                        Eligibility = "All farmers growing insured crops. Must register before sowing season.",
                        Benefits = "Coverage up to Rs. 100,000 per acre. Premium subsidy of 50%.",
                        HowToApply = "Register at local agriculture office or through partner insurance companies.",
                        ContactInfo = "PCRWR: 041-9201553",
                        StartDate = new DateTime(2018, 1, 1),
                        DisplayOrder = 4
                    },
                    new GovernmentScheme
                    {
                        Title = "Tractor Subsidy Scheme",
                        TitleUrdu = "ٹریکٹر سبسڈی اسکیم",
                        Description = "Subsidized tractors for small farmers to promote farm mechanization. The scheme provides tractors at 50% subsidized rates through government-approved dealers.",
                        DescriptionUrdu = "فارم میکنائزیشن کو فروغ دینے کے لیے چھوٹے کسانوں کے لیے سبسڈی والے ٹریکٹر۔",
                        Category = "Equipment",
                        Status = "Active",
                        Eligibility = "Small farmers with land holding less than 12.5 acres. Must not own a tractor.",
                        Benefits = "50% subsidy on tractor purchase. Priority to women farmers and youth.",
                        HowToApply = "Apply through Punjab/Sindh Agriculture Department website or local office.",
                        ContactInfo = "Agriculture Department: 042-99210431",
                        StartDate = new DateTime(2020, 1, 1),
                        DisplayOrder = 5
                    },
                    new GovernmentScheme
                    {
                        Title = "Free Seed Distribution",
                        TitleUrdu = "مفت بیج تقسیم",
                        Description = "Distribution of certified high-yield seeds to farmers free of cost or at subsidized rates. Includes wheat, rice, cotton, and vegetable seeds.",
                        DescriptionUrdu = "کسانوں کو مفت یا سبسڈی والی قیمتوں پر تصدیق شدہ زیادہ پیداوار والے بیجوں کی تقسیم۔",
                        Category = "Seeds",
                        Status = "Active",
                        Eligibility = "All registered farmers. Priority to flood-affected and low-income farmers.",
                        Benefits = "Free seeds worth Rs. 5,000 per acre. Technical guidance included.",
                        HowToApply = "Contact local agriculture extension office before sowing season.",
                        ContactInfo = "Seed Corporation: 042-35761437",
                        StartDate = new DateTime(2015, 1, 1),
                        DisplayOrder = 6
                    },
                    new GovernmentScheme
                    {
                        Title = "Farmer Training Program",
                        TitleUrdu = "کسان تربیتی پروگرام",
                        Description = "Free training programs for farmers on modern farming techniques, pest management, water conservation, and organic farming practices.",
                        DescriptionUrdu = "جدید کاشتکاری تکنیک، کیڑوں کے انتظام، پانی کے تحفظ اور نامیاتی کاشتکاری کے طریقوں پر کسانوں کے لیے مفت تربیتی پروگرام۔",
                        Category = "Training",
                        Status = "Active",
                        Eligibility = "All farmers. Youth and women farmers are encouraged.",
                        Benefits = "Free training, certificate, and sometimes daily allowance. Modern equipment demonstration.",
                        HowToApply = "Register at nearest Agricultural Extension Center or online portal.",
                        ContactInfo = "NARC: 051-9255013",
                        StartDate = new DateTime(2010, 1, 1),
                        DisplayOrder = 7
                    },
                    new GovernmentScheme
                    {
                        Title = "Fertilizer Subsidy Scheme",
                        TitleUrdu = "کھاد سبسڈی اسکیم",
                        Description = "Subsidized fertilizers including DAP, Urea, and Potash for farmers at reduced prices through authorized dealers.",
                        DescriptionUrdu = "مجاز ڈیلرز کے ذریعے کم قیمتوں پر کسانوں کے لیے ڈی اے پی، یوریا اور پوٹاش سمیت سبسڈی والی کھادیں۔",
                        Category = "Fertilizer",
                        Status = "Active",
                        Eligibility = "All farmers with valid CNIC. Quota based on land holding.",
                        Benefits = "Up to 30% subsidy on fertilizers. Direct delivery to villages.",
                        HowToApply = "Register at local dealer with CNIC and land documents.",
                        ContactInfo = "NFDC: 042-35761500",
                        StartDate = new DateTime(2022, 1, 1),
                        DisplayOrder = 8
                    }
                );

                await context.SaveChangesAsync();
            }
        }

        // ✅ Seed Fertilizer Marketplace Data
        public static async Task SeedFertilizerMarketplaceAsync(ApplicationDbContext context)
        {
            // Seed Cities (South Punjab focus)
            if (!context.Cities.Any())
            {
                context.Cities.AddRange(
                    // Large Cities
                    new City { Name = "Multan", NameUrdu = "ملتان", District = "Multan", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Bahawalpur", NameUrdu = "بہاولپور", District = "Bahawalpur", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Rahim Yar Khan", NameUrdu = "رحیم یار خان", District = "Rahim Yar Khan", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Dera Ghazi Khan", NameUrdu = "ڈیرہ غازی خان", District = "Dera Ghazi Khan", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Muzaffargarh", NameUrdu = "مظفر گڑھ", District = "Muzaffargarh", Province = "Punjab", Region = "South Punjab" },
                    
                    // Medium Cities
                    new City { Name = "Sahiwal", NameUrdu = "ساہیوال", District = "Sahiwal", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Vehari", NameUrdu = "وہاڑی", District = "Vehari", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Lodhran", NameUrdu = "لودھراں", District = "Lodhran", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Khanewal", NameUrdu = "خانیوال", District = "Khanewal", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Rajanpur", NameUrdu = "راجن پور", District = "Rajanpur", Province = "Punjab", Region = "South Punjab" },
                    
                    // Smaller Cities
                    new City { Name = "Kot Addu", NameUrdu = "کوٹ اڈو", District = "Muzaffargarh", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Jalalpur Pirwala", NameUrdu = "جلال پور پیروالا", District = "Multan", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Ahmedpur East", NameUrdu = "احمد پور شرقیہ", District = "Bahawalpur", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Liaquatpur", NameUrdu = "لیاقت پور", District = "Rahim Yar Khan", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Jampur", NameUrdu = "جام پور", District = "Rajanpur", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Taunsa", NameUrdu = "تونسہ", District = "Dera Ghazi Khan", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Kabirwala", NameUrdu = "کبیر والا", District = "Khanewal", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Minchinabad", NameUrdu = "منچن آباد", District = "Bahawalnagar", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Hasilpur", NameUrdu = "حاصل پور", District = "Bahawalpur", Province = "Punjab", Region = "South Punjab" },
                    new City { Name = "Chichawatni", NameUrdu = "چیچہ وطنی", District = "Sahiwal", Province = "Punjab", Region = "South Punjab" }
                );

                await context.SaveChangesAsync();
            }

            // Seed Categories
            if (!context.FertilizerCategories.Any())
            {
                context.FertilizerCategories.AddRange(
                    new FertilizerCategory
                    {
                        Name = "Fertilizers",
                        NameUrdu = "کھاد",
                        Description = "Chemical and organic fertilizers for crops including Urea, DAP, NPK, etc.",
                        Icon = "🌿"
                    },
                    new FertilizerCategory
                    {
                        Name = "Pesticides",
                        NameUrdu = "کیڑے مار دوا",
                        Description = "Insecticides, fungicides, and herbicides for crop protection",
                        Icon = "🪲"
                    },
                    new FertilizerCategory
                    {
                        Name = "Seeds",
                        NameUrdu = "بیج",
                        Description = "High-quality certified seeds for various crops",
                        Icon = "🌱"
                    },
                    new FertilizerCategory
                    {
                        Name = "Farming Tools",
                        NameUrdu = "زرعی آلات",
                        Description = "Hand tools and small farming equipment",
                        Icon = "🛠️"
                    },
                    new FertilizerCategory
                    {
                        Name = "Other Supplies",
                        NameUrdu = "دیگر سامان",
                        Description = "Other agricultural supplies and materials",
                        Icon = "📦"
                    }
                );

                await context.SaveChangesAsync();
            }

            // Seed Sample Products
            if (!context.FertilizerProducts.Any())
            {
                var fertilizersCategory = context.FertilizerCategories.FirstOrDefault(c => c.Name == "Fertilizers");
                var pesticidesCategory = context.FertilizerCategories.FirstOrDefault(c => c.Name == "Pesticides");
                var seedsCategory = context.FertilizerCategories.FirstOrDefault(c => c.Name == "Seeds");

                if (fertilizersCategory != null)
                {
                    context.FertilizerProducts.AddRange(
                        new FertilizerProduct
                        {
                            Name = "Engro Urea",
                            NameUrdu = "اینگرو یوریا",
                            CategoryId = fertilizersCategory.Id,
                            Brand = "Engro",
                            ManufacturerName = "Engro Fertilizers Ltd",
                            Description = "High-quality Urea fertilizer containing 46% Nitrogen. Ideal for wheat, rice, and sugarcane crops.",
                            PackageSize = "50kg",
                            Unit = "Bag",
                            ImageUrl = "/fertilizer-images/urea.jpg",
                            UsageInstructions = "Apply 2-3 bags per acre for wheat, 3-4 bags for rice."
                        },
                        new FertilizerProduct
                        {
                            Name = "FFC DAP",
                            NameUrdu = "ایف ایف سی ڈی اے پی",
                            CategoryId = fertilizersCategory.Id,
                            Brand = "FFC",
                            ManufacturerName = "Fauji Fertilizer Company",
                            Description = "Di-Ammonium Phosphate (DAP) containing 18% N and 46% P2O5. Excellent for root development.",
                            PackageSize = "50kg",
                            Unit = "Bag",
                            ImageUrl = "/fertilizer-images/dap.jpg",
                            UsageInstructions = "Apply 1-2 bags per acre at sowing time."
                        }
                    );
                }

                if (pesticidesCategory != null)
                {
                    context.FertilizerProducts.Add(
                        new FertilizerProduct
                        {
                            Name = "Confidor",
                            NameUrdu = "کنفیڈور",
                            CategoryId = pesticidesCategory.Id,
                            Brand = "Bayer",
                            ManufacturerName = "Bayer CropScience",
                            Description = "Systemic insecticide for control of sucking pests on cotton, vegetables, and fruits.",
                            PackageSize = "100ml",
                            Unit = "Bottle",
                            ImageUrl = "/fertilizer-images/confidor.jpg",
                            UsageInstructions = "Mix 10ml in 15 liters of water. Spray as needed."
                        }
                    );
                }

                await context.SaveChangesAsync();
            }

            // Seed Sample Store
            if (!context.AgriSupplyStores.Any())
            {
                var multan = context.Cities.FirstOrDefault(c => c.Name == "Multan");
                
                if (multan != null)
                {
                    context.AgriSupplyStores.Add(
                        new AgriSupplyStore
                        {
                            StoreName = "Ali Agri Store",
                            OwnerName = "Muhammad Ali",
                            CityId = multan.Id,
                            Address = "Main Bosan Road, Multan",
                            ContactNumber = "061-4567890",
                            WhatsAppNumber = "0300-1234567",
                            Email = "ali@agristore.com",
                            IsVerified = true
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
