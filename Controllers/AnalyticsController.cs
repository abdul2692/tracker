using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;

namespace SpendingTracker.Controllers
{
    [Authorize]
    public class AnalyticsController : BaseController
    {
        private readonly IExpenseService _expenseService;
        private readonly IIncomeService _incomeService;

        public AnalyticsController(IExpenseService expenseService, IIncomeService incomeService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _expenseService = expenseService;
            _incomeService = incomeService;
        }

        public async Task<IActionResult> Index(int? month = null, int? year = null)
        {
            var userId = _userManager.GetUserId(User)!;
            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            var from = new DateTime(y, m, 1);
            var to = from.AddMonths(1).AddDays(-1);

            var expenses = (await _expenseService.GetByDateRangeAsync(userId, from, to)).ToList();

            // Group by category, sum amount, sort descending
            var categoryStats = expenses
                .GroupBy(e => new { e.Category.Name, e.Category.Color, e.Category.Icon })
                .Select(g => new
                {
                    Category = g.Key.Name,
                    Amount = g.Sum(e => e.Amount),
                    Color = g.Key.Color,
                    Icon = g.Key.Icon
                })
                .Where(c => c.Amount > 0)
                .OrderByDescending(c => c.Amount)
                .ToList();

            // 12-Month trend
            var trendAnchor = new DateTime(y, m, 1);
            var trend = new List<object>();
            for (int i = 11; i >= 0; i--)
            {
                var date = trendAnchor.AddMonths(-i);
                var income = await _incomeService.GetMonthlyIncomeAsync(userId, date.Month, date.Year);
                var mExpenses = await _expenseService.GetMonthlyExpensesAsync(userId, date.Month, date.Year);
                trend.Add(new { Month = date.ToString("MMM yy"), Income = income, Expenses = mExpenses });
            }

            ViewBag.SelectedMonth = m;
            ViewBag.SelectedYear = y;
            ViewBag.CategoryStatsJson = System.Text.Json.JsonSerializer.Serialize(categoryStats);
            ViewBag.MonthlyTrendJson = System.Text.Json.JsonSerializer.Serialize(trend);

            return View(categoryStats);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryData(int? month = null, int? year = null)
        {
            var userId = _userManager.GetUserId(User)!;
            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            var from = new DateTime(y, m, 1);
            var to = from.AddMonths(1).AddDays(-1);

            var expenses = (await _expenseService.GetByDateRangeAsync(userId, from, to)).ToList();

            var categoryStats = expenses
                .GroupBy(e => new { e.Category.Name, e.Category.Color, e.Category.Icon })
                .Select(g => new
                {
                    Category = g.Key.Name,
                    Amount = g.Sum(e => e.Amount),
                    Color = g.Key.Color,
                    Icon = g.Key.Icon
                })
                .Where(c => c.Amount > 0)
                .OrderByDescending(c => c.Amount)
                .ToList();

            return Json(categoryStats);
        }
    }
}
