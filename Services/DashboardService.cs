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

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var now = DateTime.Now;
            var vm = new DashboardViewModel();

            vm.TotalIncome = await _incomeService.GetTotalIncomeAsync(userId);
            vm.TotalExpenses = await _expenseService.GetTotalExpensesAsync(userId);
            vm.MonthlyIncome = await _incomeService.GetMonthlyIncomeAsync(userId, now.Month, now.Year);
            vm.MonthlyExpenses = await _expenseService.GetMonthlyExpensesAsync(userId, now.Month, now.Year);
            vm.RecentExpenses = await _expenseService.GetRecentExpensesAsync(userId, 8);
            vm.ExpensesByCategory = await _expenseService.GetExpensesByCategoryAsync(userId);
            vm.BudgetStatuses = await _budgetService.GetBudgetStatusAsync(userId, now.Month, now.Year);

            if (vm.ExpensesByCategory.Any())
            {
                var top = vm.ExpensesByCategory.MaxBy(kv => kv.Value);
                vm.HighestSpendingCategory = top.Key;
                vm.HighestSpendingAmount = top.Value;
            }

            // Build 6-month trend
            vm.MonthlyTrend = new List<MonthlyTrendPoint>();
            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
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
