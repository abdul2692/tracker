using SpendingTracker.Models;

namespace SpendingTracker.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance => TotalIncome - TotalExpenses;

        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal MonthlyBalance => MonthlyIncome - MonthlyExpenses;

        public string HighestSpendingCategory { get; set; } = "N/A";
        public decimal HighestSpendingAmount { get; set; }

        public IEnumerable<Expense> RecentExpenses { get; set; } = new List<Expense>();
        public IEnumerable<BudgetStatusViewModel> BudgetStatuses { get; set; } = new List<BudgetStatusViewModel>();

        // Chart data
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
        public List<MonthlyTrendPoint> MonthlyTrend { get; set; } = new();
    }

    public class MonthlyTrendPoint
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }
}
