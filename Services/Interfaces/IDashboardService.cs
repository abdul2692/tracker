using SpendingTracker.ViewModels;

namespace SpendingTracker.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string userId, int? month = null, int? year = null);
    }
}
