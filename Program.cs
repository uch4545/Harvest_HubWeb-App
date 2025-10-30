using Data;
using HarvestHub.WebApp.Data;
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using HarvestHub.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Entity Framework & Identity services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // ✅ Password Policy
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // ✅ Needed for OTP, password reset etc.

// ✅ Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// ✅ Add MVC + API Controllers
builder.Services.AddControllersWithViews();
builder.Services.AddControllers(); // 👈 for API routes

// ✅ Add Market Rate Service (HTTP Client)
builder.Services.AddHttpClient<MarketRateService>(client =>
{
    // ⚙️ Replace this port with your own project's HTTPS port
    // check from launchSettings.json or browser URL
    client.BaseAddress = new Uri("https://localhost:7290/");
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});


// ✅ Build the app
var app = builder.Build();

// ✅ HTTP pipeline configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();

app.UseRouting();

// ✅ Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Map API Controllers
app.MapControllers();

// ✅ Seed Roles, Admin User & Market Rates
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    await SeedData.SeedRolesAndAdminAsync(userManager, roleManager);
    await SeedData.SeedMarketRatesAsync(context); // 👈 added this line
}

// ✅ Run the app
app.Run();
