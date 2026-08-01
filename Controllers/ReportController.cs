using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;

namespace SpendingTracker.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(IReportService reportService, UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string period = "Monthly", int? month = null, int? year = null)
        {
            var userId = _userManager.GetUserId(User)!;
            var reportPeriod = Enum.Parse<ReportPeriod>(period, true);
            var vm = await _reportService.GenerateReportAsync(userId, reportPeriod, month, year);
            ViewBag.Period = period;
            return View(vm);
        }
    }
}
