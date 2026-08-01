using SpendingTracker.Models;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<IEnumerable<Expense>> GetUserExpensesAsync(string userId);
        Task<IEnumerable<Expense>> GetUserExpensesPagedAsync(string userId, int page, int pageSize, string? search, string? sortBy, int? categoryId);
        Task<Expense?> GetExpenseByIdAsync(int id, string userId);
        Task CreateExpenseAsync(ExpenseViewModel vm, string userId);
        Task UpdateExpenseAsync(ExpenseViewModel vm, string userId);
        Task DeleteExpenseAsync(int id, string userId);
        Task<decimal> GetTotalExpensesAsync(string userId);
        Task<decimal> GetMonthlyExpensesAsync(string userId, int month, int year);
        Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(string userId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<Expense>> GetRecentExpensesAsync(string userId, int count = 10);
        Task<IEnumerable<Expense>> GetByDateRangeAsync(string userId, DateTime from, DateTime to);
    }
}
