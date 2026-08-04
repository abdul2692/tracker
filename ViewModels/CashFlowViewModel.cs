namespace SpendingTracker.ViewModels
{
    public class CashFlowViewModel
    {
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetCashFlow => TotalIncome - TotalExpenses;

        public bool HasData => TotalIncome > 0 || TotalExpenses > 0;

        // Daily / Weekly breakdown points for Income vs Expense Chart
        public List<CashFlowPoint> BreakdownPoints { get; set; } = new();

        // 12-Month Cash Flow Trend points
        public List<CashFlowTrendPoint> MonthlyTrend { get; set; } = new();
    }

    public class CashFlowPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Net => Income - Expenses;
    }

    public class CashFlowTrendPoint
    {
        public string MonthLabel { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetCashFlow { get; set; }
    }
}
