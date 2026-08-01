using SpendingTracker.Models;

namespace SpendingTracker.Repositories.Interfaces
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Expense>> GetByUserIdAndDateRangeAsync(string userId, DateTime from, DateTime to);
        Task<IEnumerable<Expense>> GetByUserIdAndCategoryAsync(string userId, int categoryId);
        Task<decimal> GetTotalByUserIdAsync(string userId);
        Task<decimal> GetTotalByUserIdAndMonthAsync(string userId, int month, int year);
        Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(string userId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<Expense>> GetRecentByUserIdAsync(string userId, int count = 10);
    }
}
