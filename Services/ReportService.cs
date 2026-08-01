using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services
{
    public class ReportService : IReportService
    {
        private readonly IIncomeService _incomeService;
        private readonly IExpenseService _expenseService;

        public ReportService(IIncomeService incomeService, IExpenseService expenseService)
        {
            _incomeService = incomeService;
            _expenseService = expenseService;
        }

        public async Task<ReportViewModel> GenerateReportAsync(string userId, ReportPeriod period, int? month = null, int? year = null)
        {
            var now = DateTime.Now;
            var vm = new ReportViewModel
            {
                Period = period,
                Month = month,
                Year = year
            };

            DateTime from, to;

            switch (period)
            {
                case ReportPeriod.Weekly:
                    var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                    from = startOfWeek;
                    to = startOfWeek.AddDays(6);
                    vm.PeriodLabel = $"Week of {from:MMM dd} – {to:MMM dd, yyyy}";
                    break;
                case ReportPeriod.Yearly:
                    int yr = year ?? now.Year;
                    from = new DateTime(yr, 1, 1);
                    to = new DateTime(yr, 12, 31);
                    vm.PeriodLabel = $"Year {yr}";
                    break;
                default: // Monthly
                    int m = month ?? now.Month;
                    int y = year ?? now.Year;
                    from = new DateTime(y, m, 1);
                    to = from.AddMonths(1).AddDays(-1);
                    vm.PeriodLabel = $"{from:MMMM yyyy}";
                    break;
            }

            vm.FromDate = from;
            vm.ToDate = to;

            vm.Incomes = await _incomeService.GetByDateRangeAsync(userId, from, to);
            vm.Expenses = await _expenseService.GetByDateRangeAsync(userId, from, to);
            vm.TotalIncome = vm.Incomes.Sum(i => i.Amount);
            vm.TotalExpenses = vm.Expenses.Sum(e => e.Amount);
            vm.ExpensesByCategory = await _expenseService.GetExpensesByCategoryAsync(userId, from, to);

            // Weekly breakdown for monthly report
            if (period == ReportPeriod.Monthly || period == ReportPeriod.Yearly)
            {
                var trend = new List<MonthlyTrendPoint>();
                var cursor = from;
                while (cursor <= to)
                {
                    var weekEnd = cursor.AddDays(6) > to ? to : cursor.AddDays(6);
                    var weekIncomes = vm.Incomes.Where(i => i.Date >= cursor && i.Date <= weekEnd).Sum(i => i.Amount);
                    var weekExpenses = vm.Expenses.Where(e => e.Date >= cursor && e.Date <= weekEnd).Sum(e => e.Amount);
                    trend.Add(new MonthlyTrendPoint
                    {
                        Month = cursor.ToString("MMM dd"),
                        Income = weekIncomes,
                        Expenses = weekExpenses
                    });
                    cursor = weekEnd.AddDays(1);
                }
                vm.WeeklyOrMonthlyTrend = trend;
            }

            return vm;
        }
    }
}
