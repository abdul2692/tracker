using SpendingTracker.Repositories.Interfaces;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IIncomeService _incomeService;
        private readonly IExpenseService _expenseService;
        private readonly IBudgetService _budgetService;

        public DashboardService(IIncomeService incomeService, IExpenseService expenseService, IBudgetService budgetService)
        {
            _incomeService = incomeService;
            _expenseService = expenseService;
            _budgetService = budgetService;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId, int? month = null, int? year = null)
        {
            var now = DateTime.Now;
            int targetMonth = month ?? now.Month;
            int targetYear = year ?? now.Year;

            var vm = new DashboardViewModel
            {
                SelectedMonth = targetMonth,
                SelectedYear = targetYear
            };

            var monthFrom = new DateTime(targetYear, targetMonth, 1);
            var monthTo = monthFrom.AddMonths(1).AddDays(-1);

            vm.MonthlyIncome = await _incomeService.GetMonthlyIncomeAsync(userId, targetMonth, targetYear);
            vm.MonthlyExpenses = await _expenseService.GetMonthlyExpensesAsync(userId, targetMonth, targetYear);

            vm.TotalIncome = vm.MonthlyIncome;
            vm.TotalExpenses = vm.MonthlyExpenses;

            var monthExpensesList = (await _expenseService.GetByDateRangeAsync(userId, monthFrom, monthTo)).ToList();
            vm.RecentExpenses = monthExpensesList.OrderByDescending(e => e.Date).Take(8);
            vm.ExpensesByCategory = await _expenseService.GetExpensesByCategoryAsync(userId, monthFrom, monthTo);
            vm.BudgetStatuses = await _budgetService.GetBudgetStatusAsync(userId, targetMonth, targetYear);

            if (vm.ExpensesByCategory.Any())
            {
                var top = vm.ExpensesByCategory.MaxBy(kv => kv.Value);
                vm.HighestSpendingCategory = top.Key;
                vm.HighestSpendingAmount = top.Value;
            }
            else
            {
                vm.HighestSpendingCategory = "N/A";
                vm.HighestSpendingAmount = 0;
            }

            // Build 6-month trend ending at target period
            vm.MonthlyTrend = new List<MonthlyTrendPoint>();
            var endDate = new DateTime(targetYear, targetMonth, 1);
            for (int i = 5; i >= 0; i--)
            {
                var date = endDate.AddMonths(-i);
                var income = await _incomeService.GetMonthlyIncomeAsync(userId, date.Month, date.Year);
                var expenses = await _expenseService.GetMonthlyExpensesAsync(userId, date.Month, date.Year);
                vm.MonthlyTrend.Add(new MonthlyTrendPoint
                {
                    Month = date.ToString("MMM yyyy"),
                    Income = income,
                    Expenses = expenses
                });
            }

            return vm;
        }
    }
}
