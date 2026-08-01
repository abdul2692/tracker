using SpendingTracker.Models;
using SpendingTracker.Repositories.Interfaces;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly IIncomeRepository _repo;

        public IncomeService(IIncomeRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Income>> GetUserIncomesAsync(string userId)
            => await _repo.GetByUserIdAsync(userId);

        public async Task<IEnumerable<Income>> GetUserIncomesPagedAsync(string userId, int page, int pageSize, string? search, string? sortBy)
        {
            var all = await _repo.GetByUserIdAsync(userId);

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(i =>
                    i.Source.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (i.Description != null && i.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));

            all = sortBy switch
            {
                "amount_asc" => all.OrderBy(i => i.Amount),
                "amount_desc" => all.OrderByDescending(i => i.Amount),
                "date_asc" => all.OrderBy(i => i.Date),
                _ => all.OrderByDescending(i => i.Date)
            };

            return all.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public async Task<Income?> GetIncomeByIdAsync(int id, string userId)
        {
            var income = await _repo.GetByIdAsync(id);
            return income?.UserId == userId ? income : null;
        }

        public async Task CreateIncomeAsync(IncomeViewModel vm, string userId)
        {
            var income = new Income
            {
                Amount = vm.Amount,
                Source = vm.Source,
                Date = vm.Date,
                Description = vm.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(income);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateIncomeAsync(IncomeViewModel vm, string userId)
        {
            var income = await _repo.GetByIdAsync(vm.Id);
            if (income == null || income.UserId != userId) return;

            income.Amount = vm.Amount;
            income.Source = vm.Source;
            income.Date = vm.Date;
            income.Description = vm.Description;
            income.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(income);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteIncomeAsync(int id, string userId)
        {
            var income = await _repo.GetByIdAsync(id);
            if (income == null || income.UserId != userId) return;
            await _repo.DeleteAsync(income);
            await _repo.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalIncomeAsync(string userId)
            => await _repo.GetTotalByUserIdAsync(userId);

        public async Task<decimal> GetMonthlyIncomeAsync(string userId, int month, int year)
            => await _repo.GetTotalByUserIdAndMonthAsync(userId, month, year);

        public async Task<IEnumerable<Income>> GetByDateRangeAsync(string userId, DateTime from, DateTime to)
            => await _repo.GetByUserIdAndDateRangeAsync(userId, from, to);
    }
}
