using Microsoft.AspNetCore.Mvc;

namespace Harvest_Hub.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message ?? "An unexpected error occurred.";
            return View();
        }
    }
}
