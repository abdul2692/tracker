using Microsoft.EntityFrameworkCore;
using SpendingTracker.Data;
using SpendingTracker.Models;
using SpendingTracker.Repositories.Interfaces;

namespace SpendingTracker.Repositories
{
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Expense>> GetByUserIdAsync(string userId)
            => await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

        public async Task<IEnumerable<Expense>> GetByUserIdAndDateRangeAsync(string userId, DateTime from, DateTime to)
            => await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.Date >= from && e.Date <= to)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

        public async Task<IEnumerable<Expense>> GetByUserIdAndCategoryAsync(string userId, int categoryId)
            => await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.CategoryId == categoryId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

        public async Task<decimal> GetTotalByUserIdAsync(string userId)
            => (decimal)await _context.Expenses
                .Where(e => e.UserId == userId)
                .SumAsync(e => (double)e.Amount);

        public async Task<decimal> GetTotalByUserIdAndMonthAsync(string userId, int month, int year)
            => (decimal)await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == month && e.Date.Year == year)
                .SumAsync(e => (double)e.Amount);

        public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(string userId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            if (from.HasValue) query = query.Where(e => e.Date >= from.Value);
            if (to.HasValue) query = query.Where(e => e.Date <= to.Value);

            return await query
                .GroupBy(e => e.Category.Name)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => (double)e.Amount) })
                .ToDictionaryAsync(x => x.Category, x => (decimal)x.Total);
        }

        public async Task<IEnumerable<Expense>> GetRecentByUserIdAsync(string userId, int count = 10)
            => await _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(count)
                .ToListAsync();
    }
}
