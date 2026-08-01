using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;

namespace SpendingTracker.ViewModels
{
    public class ReportViewModel
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public ReportPeriod Period { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetSavings => TotalIncome - TotalExpenses;

        public IEnumerable<Income> Incomes { get; set; } = new List<Income>();
        public IEnumerable<Expense> Expenses { get; set; } = new List<Expense>();

        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
        public List<MonthlyTrendPoint> WeeklyOrMonthlyTrend { get; set; } = new();
    }
}
