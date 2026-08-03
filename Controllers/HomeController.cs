using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _dashboardService.GetDashboardDataAsync(userId);
            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Error(int? statusCode = null)
        {
            return View("~/Views/Shared/Error.cshtml", statusCode?.ToString() ?? "500");
        }
    }
}
