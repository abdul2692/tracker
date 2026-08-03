using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;

namespace SpendingTracker.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _reportService = reportService;
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
