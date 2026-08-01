using SpendingTracker.Models;

namespace SpendingTracker.Repositories.Interfaces
{
    public interface IBudgetRepository : IRepository<Budget>
    {
        Task<IEnumerable<Budget>> GetByUserIdAsync(string userId);
        Task<Budget?> GetMonthlyBudgetAsync(string userId, int month, int year);
        Task<IEnumerable<Budget>> GetCategoryBudgetsAsync(string userId);
    }
}
