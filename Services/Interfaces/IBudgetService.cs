using SpendingTracker.Models;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services.Interfaces
{
    public interface IBudgetService
    {
        Task<IEnumerable<Budget>> GetUserBudgetsAsync(string userId);
        Task<Budget?> GetByIdAsync(int id, string userId);
        Task CreateBudgetAsync(BudgetViewModel vm, string userId);
        Task UpdateBudgetAsync(BudgetViewModel vm, string userId);
        Task DeleteBudgetAsync(int id, string userId);
        Task<IEnumerable<BudgetStatusViewModel>> GetBudgetStatusAsync(string userId, int month, int year);
    }
}
