using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest_Hub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ErrorLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ErrorLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.ErrorLogs
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return View(logs);
        }
    }
}
