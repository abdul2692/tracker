using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SpendingTracker.Models;

namespace SpendingTracker.Controllers
{
    public class BaseController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        private static readonly Dictionary<string, string> CurrencySymbols = new(StringComparer.OrdinalIgnoreCase)
        {
            { "USD", "$" },
            { "GBP", "£" },
            { "EUR", "€" },
            { "INR", "₹" },
            { "PKR", "Rs" },
            { "AED", "د.إ" },
            { "CAD", "CA$" },
            { "AUD", "A$" }
        };

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                var code = user?.Currency ?? "GBP";
                var symbol = CurrencySymbols.TryGetValue(code, out var s) ? s : code;

                ViewBag.CurrencyCode   = code;
                ViewBag.CurrencySymbol = symbol;
                ViewBag.ProfilePicture = user?.ProfilePicture;
                ViewBag.UserFullName   = user != null && !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : User.Identity.Name;
            }
            else
            {
                ViewBag.CurrencyCode   = "GBP";
                ViewBag.CurrencySymbol = "\u00A3";
                ViewBag.ProfilePicture = null;
                ViewBag.UserFullName   = "";
            }

            await next();
        }
    }
}