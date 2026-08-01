using SpendingTracker.ViewModels;

namespace SpendingTracker.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportViewModel> GenerateReportAsync(string userId, ReportPeriod period, int? month = null, int? year = null);
    }

    public enum ReportPeriod
    {
        Weekly,
        Monthly,
        Yearly
    }
}
