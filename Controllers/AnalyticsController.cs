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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var now = DateTime.Now;

            var expensesByCategory = await _expenseService.GetExpensesByCategoryAsync(userId);

            // Monthly trend - 12 months
            var trend = new List<object>();
            for (int i = 11; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var income = await _incomeService.GetMonthlyIncomeAsync(userId, date.Month, date.Year);
                var expenses = await _expenseService.GetMonthlyExpensesAsync(userId, date.Month, date.Year);
                trend.Add(new { Month = date.ToString("MMM yy"), Income = income, Expenses = expenses });
            }

            ViewBag.ExpensesByCategory = System.Text.Json.JsonSerializer.Serialize(expensesByCategory);
            ViewBag.MonthlyTrend = System.Text.Json.JsonSerializer.Serialize(trend);

            return View();
        }
    }
}
