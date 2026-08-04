using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Controllers
{
    [Authorize]
    public class CashFlowController : BaseController
    {
        private readonly IIncomeService _incomeService;
        private readonly IExpenseService _expenseService;

        public CashFlowController(
            IIncomeService incomeService,
            IExpenseService expenseService,
            UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _incomeService = incomeService;
            _expenseService = expenseService;
        }

        public async Task<IActionResult> Index(int? month = null, int? year = null)
        {
            var userId = _userManager.GetUserId(User)!;
            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            var from = new DateTime(y, m, 1);
            var to = from.AddMonths(1).AddDays(-1);

            var incomes = (await _incomeService.GetByDateRangeAsync(userId, from, to)).ToList();
            var expenses = (await _expenseService.GetByDateRangeAsync(userId, from, to)).ToList();

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpenses = expenses.Sum(e => e.Amount);

            var vm = new CashFlowViewModel
            {
                SelectedMonth = m,
                SelectedYear = y,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses
            };

            // Build Daily Breakdown for the selected month
            int daysInMonth = DateTime.DaysInMonth(y, m);
            for (int day = 1; day <= daysInMonth; day++)
            {
                var curDate = new DateTime(y, m, day);
                var dayInc = incomes.Where(i => i.Date.Date == curDate.Date).Sum(i => i.Amount);
                var dayExp = expenses.Where(e => e.Date.Date == curDate.Date).Sum(e => e.Amount);

                vm.BreakdownPoints.Add(new CashFlowPoint
                {
                    Label = curDate.ToString("MMM dd"),
                    Income = dayInc,
                    Expenses = dayExp
                });
            }

            // Build 12-Month Trend ending at selected period
            var trendAnchor = new DateTime(y, m, 1);
            for (int i = 11; i >= 0; i--)
            {
                var date = trendAnchor.AddMonths(-i);
                var mInc = await _incomeService.GetMonthlyIncomeAsync(userId, date.Month, date.Year);
                var mExp = await _expenseService.GetMonthlyExpensesAsync(userId, date.Month, date.Year);

                vm.MonthlyTrend.Add(new CashFlowTrendPoint
                {
                    MonthLabel = date.ToString("MMM yyyy"),
                    Income = mInc,
                    Expenses = mExp,
                    NetCashFlow = mInc - mExp
                });
            }

            return View(vm);
        }
    }
}
