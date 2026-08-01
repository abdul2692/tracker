using SpendingTracker.Models;
using SpendingTracker.Repositories.Interfaces;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repo;

        public ExpenseService(IExpenseRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Expense>> GetUserExpensesAsync(string userId)
            => await _repo.GetByUserIdAsync(userId);

        public async Task<IEnumerable<Expense>> GetUserExpensesPagedAsync(string userId, int page, int pageSize, string? search, string? sortBy, int? categoryId)
        {
            var all = await _repo.GetByUserIdAsync(userId);

            if (categoryId.HasValue)
                all = all.Where(e => e.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(e =>
                    (e.Description != null && e.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    e.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            all = sortBy switch
            {
                "amount_asc" => all.OrderBy(e => e.Amount),
                "amount_desc" => all.OrderByDescending(e => e.Amount),
                "date_asc" => all.OrderBy(e => e.Date),
                "category" => all.OrderBy(e => e.Category.Name),
                _ => all.OrderByDescending(e => e.Date)
            };

            return all.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public async Task<Expense?> GetExpenseByIdAsync(int id, string userId)
        {
            var expense = await _repo.GetByIdAsync(id);
            return expense?.UserId == userId ? expense : null;
        }

        public async Task CreateExpenseAsync(ExpenseViewModel vm, string userId)
        {
            var expense = new Expense
            {
                Amount = vm.Amount,
                CategoryId = vm.CategoryId,
                Date = vm.Date,
                Description = vm.Description,
                PaymentMethod = vm.PaymentMethod,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(expense);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateExpenseAsync(ExpenseViewModel vm, string userId)
        {
            var expense = await _repo.GetByIdAsync(vm.Id);
            if (expense == null || expense.UserId != userId) return;

            expense.Amount = vm.Amount;
            expense.CategoryId = vm.CategoryId;
            expense.Date = vm.Date;
            expense.Description = vm.Description;
            expense.PaymentMethod = vm.PaymentMethod;
            expense.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(expense);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteExpenseAsync(int id, string userId)
        {
            var expense = await _repo.GetByIdAsync(id);
            if (expense == null || expense.UserId != userId) return;
            await _repo.DeleteAsync(expense);
            await _repo.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalExpensesAsync(string userId)
            => await _repo.GetTotalByUserIdAsync(userId);

        public async Task<decimal> GetMonthlyExpensesAsync(string userId, int month, int year)
            => await _repo.GetTotalByUserIdAndMonthAsync(userId, month, year);

        public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(string userId, DateTime? from = null, DateTime? to = null)
            => await _repo.GetExpensesByCategoryAsync(userId, from, to);

        public async Task<IEnumerable<Expense>> GetRecentExpensesAsync(string userId, int count = 10)
            => await _repo.GetRecentByUserIdAsync(userId, count);

        public async Task<IEnumerable<Expense>> GetByDateRangeAsync(string userId, DateTime from, DateTime to)
            => await _repo.GetByUserIdAndDateRangeAsync(userId, from, to);
    }
}
