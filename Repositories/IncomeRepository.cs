using Microsoft.EntityFrameworkCore;
using SpendingTracker.Data;
using SpendingTracker.Models;
using SpendingTracker.Repositories.Interfaces;

namespace SpendingTracker.Repositories
{
    public class IncomeRepository : Repository<Income>, IIncomeRepository
    {
        public IncomeRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Income>> GetByUserIdAsync(string userId)
            => await _context.Incomes
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

        public async Task<IEnumerable<Income>> GetByUserIdAndDateRangeAsync(string userId, DateTime from, DateTime to)
            => await _context.Incomes
                .Where(i => i.UserId == userId && i.Date >= from && i.Date <= to)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

        public async Task<decimal> GetTotalByUserIdAsync(string userId)
            => (decimal)await _context.Incomes
                .Where(i => i.UserId == userId)
                .SumAsync(i => (double)i.Amount);

        public async Task<decimal> GetTotalByUserIdAndMonthAsync(string userId, int month, int year)
            => (decimal)await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == month && i.Date.Year == year)
                .SumAsync(i => (double)i.Amount);
    }
}
