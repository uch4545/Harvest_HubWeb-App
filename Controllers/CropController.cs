
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Data;

[Authorize(Roles = "Farmer")]
public class CropController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CropController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        try
        {
            var crops = _context.Crops.ToList();
            return View(crops);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while loading crops. Please try again.";
            LogError("Index", ex);
            return RedirectToAction("Dashboard", "Farmer");
        }
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Crop crop)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var farmer = _context.Farmers.FirstOrDefault(f => f.ApplicationUserId == user.Id);
            if (farmer == null) return Unauthorized();

            crop.FarmerId = farmer.Id;
            _context.Crops.Add(crop);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Crop created successfully!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while creating crop. Please try again.";
            LogError("Create", ex);
            return View(crop);
        }
    }

    #region ---------------- Error Logging ----------------
    private void LogError(string actionName, Exception ex)
    {
        try
        {
            var error = new ErrorLog
            {
                ControllerName = nameof(CropController),
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

