using Microsoft.EntityFrameworkCore;
using SpendingTracker.Data;
using SpendingTracker.Models;
using SpendingTracker.Repositories.Interfaces;

namespace SpendingTracker.Repositories
{
    public class BudgetRepository : Repository<Budget>, IBudgetRepository
    {
        public BudgetRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Budget>> GetByUserIdAsync(string userId)
            => await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<Budget?> GetMonthlyBudgetAsync(string userId, int month, int year)
            => await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId
                    && b.BudgetType == BudgetType.Monthly
                    && b.Month == month
                    && b.Year == year);

        public async Task<IEnumerable<Budget>> GetCategoryBudgetsAsync(string userId)
            => await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.BudgetType == BudgetType.Category)
                .ToListAsync();
    }
}
