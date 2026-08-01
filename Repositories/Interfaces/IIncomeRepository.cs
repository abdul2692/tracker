using SpendingTracker.Models;

namespace SpendingTracker.Repositories.Interfaces
{
    public interface IIncomeRepository : IRepository<Income>
    {
        Task<IEnumerable<Income>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Income>> GetByUserIdAndDateRangeAsync(string userId, DateTime from, DateTime to);
        Task<decimal> GetTotalByUserIdAsync(string userId);
        Task<decimal> GetTotalByUserIdAndMonthAsync(string userId, int month, int year);
    }
}
