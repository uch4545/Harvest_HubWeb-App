using Data;
using HarvestHub.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HarvestHub.WebApp.Data
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ApplicationDbContext _context;

        public CustomExceptionFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnException(ExceptionContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var error = new ErrorLog
            {
                ControllerName = controller,
                ActionName = action,
                UserId = userId,
                ExceptionMessage = context.Exception.Message,
                StackTrace = context.Exception.StackTrace
            };

            _context.ErrorLogs.Add(error);
            _context.SaveChanges();

            context.Result = new RedirectToActionResult(
                "Error", "Home",
                new { message = "Something went wrong. Please try again later." });

            context.ExceptionHandled = true;
        }
    }
}
