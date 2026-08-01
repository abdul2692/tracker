using SpendingTracker.Models;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services.Interfaces
{
    public interface IIncomeService
    {
        Task<IEnumerable<Income>> GetUserIncomesAsync(string userId);
        Task<IEnumerable<Income>> GetUserIncomesPagedAsync(string userId, int page, int pageSize, string? search, string? sortBy);
        Task<Income?> GetIncomeByIdAsync(int id, string userId);
        Task CreateIncomeAsync(IncomeViewModel vm, string userId);
        Task UpdateIncomeAsync(IncomeViewModel vm, string userId);
        Task DeleteIncomeAsync(int id, string userId);
        Task<decimal> GetTotalIncomeAsync(string userId);
        Task<decimal> GetMonthlyIncomeAsync(string userId, int month, int year);
        Task<IEnumerable<Income>> GetByDateRangeAsync(string userId, DateTime from, DateTime to);
    }
}
