using Microsoft.AspNetCore.Mvc;

namespace Harvest_Hub.Controllers
{
    public class HomeController : Controller
    {
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Farmer"))
                    return RedirectToAction("Dashboard", "Farmer");
                if (User.IsInRole("Buyer"))
                    return RedirectToAction("Dashboard", "Buyer");
            }
            return View();
        }

        public IActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message ?? "An unexpected error occurred.";
            return View();
        }
    }
}
