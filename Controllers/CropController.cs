
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
        var crops = _context.Crops.ToList();
        return View(crops);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Crop crop)
    {
        var user = await _userManager.GetUserAsync(User);
        var farmer = _context.Farmers.FirstOrDefault(f => f.ApplicationUserId == user.Id);
        if (farmer == null) return Unauthorized();

        crop.FarmerId = farmer.Id;
        _context.Crops.Add(crop);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
