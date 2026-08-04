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
        private readonly IExpenseService _expenseService;
        private readonly IIncomeService _incomeService;

        public HomeController(
            IDashboardService dashboardService,
            IExpenseService expenseService,
            IIncomeService incomeService,
            UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _dashboardService = dashboardService;
            _expenseService = expenseService;
            _incomeService = incomeService;
        }

        public async Task<IActionResult> Index(int? month = null, int? year = null)
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _dashboardService.GetDashboardDataAsync(userId, month, year);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string view = "Daily", int? month = null, int? year = null)
        {
            var userId = _userManager.GetUserId(User)!;
            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            if (view.Equals("Daily", StringComparison.OrdinalIgnoreCase))
            {
                int daysInMonth = DateTime.DaysInMonth(y, m);
                var from = new DateTime(y, m, 1);
                var to = from.AddMonths(1).AddDays(-1);

                var expenses = (await _expenseService.GetByDateRangeAsync(userId, from, to)).ToList();
                var incomes = (await _incomeService.GetByDateRangeAsync(userId, from, to)).ToList();

                var labels = new List<string>();
                var expenseData = new List<decimal>();
                var incomeData = new List<decimal>();

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var currentDate = new DateTime(y, m, day);
                    labels.Add(currentDate.ToString("MMM dd"));

                    var dayExpenses = expenses.Where(e => e.Date.Date == currentDate.Date).Sum(e => e.Amount);
                    var dayIncomes = incomes.Where(i => i.Date.Date == currentDate.Date).Sum(i => i.Amount);

                    expenseData.Add(dayExpenses);
                    incomeData.Add(dayIncomes);
                }

                return Json(new { labels, expenseData, incomeData });
            }
            else if (view.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
            {
                var from = new DateTime(y, m, 1);
                var to = from.AddMonths(1).AddDays(-1);

                var expenses = (await _expenseService.GetByDateRangeAsync(userId, from, to)).ToList();
                var incomes = (await _incomeService.GetByDateRangeAsync(userId, from, to)).ToList();

                var labels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4", "Week 5" };
                var expenseData = new decimal[5];
                var incomeData = new decimal[5];

                foreach (var exp in expenses)
                {
                    int weekIndex = Math.Min(4, (exp.Date.Day - 1) / 7);
                    expenseData[weekIndex] += exp.Amount;
                }

                foreach (var inc in incomes)
                {
                    int weekIndex = Math.Min(4, (inc.Date.Day - 1) / 7);
                    incomeData[weekIndex] += inc.Amount;
                }

                return Json(new { labels, expenseData, incomeData });
            }
            else // Monthly View
            {
                var labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                var expenseData = new List<decimal>();
                var incomeData = new List<decimal>();

                for (int monthIdx = 1; monthIdx <= 12; monthIdx++)
                {
                    var expTotal = await _expenseService.GetMonthlyExpensesAsync(userId, monthIdx, y);
                    var incTotal = await _incomeService.GetMonthlyIncomeAsync(userId, monthIdx, y);

                    expenseData.Add(expTotal);
                    incomeData.Add(incTotal);
                }

                return Json(new { labels, expenseData, incomeData });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardJson(int? month = null, int? year = null)
        {
            var userId = _userManager.GetUserId(User)!;
            var vm = await _dashboardService.GetDashboardDataAsync(userId, month, year);
            return Json(vm);
        }

        [AllowAnonymous]
        public IActionResult Error(int? statusCode = null)
        {
            return View("~/Views/Shared/Error.cshtml", statusCode?.ToString() ?? "500");
        }
    }
}
