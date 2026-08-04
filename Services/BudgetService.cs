using SpendingTracker.Models;
using SpendingTracker.Repositories.Interfaces;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepo;
        private readonly IExpenseRepository _expenseRepo;

        public BudgetService(IBudgetRepository budgetRepo, IExpenseRepository expenseRepo)
        {
            _budgetRepo = budgetRepo;
            _expenseRepo = expenseRepo;
        }

        public async Task<IEnumerable<Budget>> GetUserBudgetsAsync(string userId)
            => await _budgetRepo.GetByUserIdAsync(userId);

        public async Task<Budget?> GetByIdAsync(int id, string userId)
        {
            var budget = await _budgetRepo.GetByIdAsync(id);
            return budget?.UserId == userId ? budget : null;
        }

        public async Task CreateBudgetAsync(BudgetViewModel vm, string userId)
        {
            var budget = new Budget
            {
                Name = vm.Name,
                Amount = vm.Amount,
                BudgetType = vm.BudgetType,
                Month = vm.Month,
                Year = vm.Year,
                CategoryId = vm.CategoryId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _budgetRepo.AddAsync(budget);
            await _budgetRepo.SaveChangesAsync();
        }

        public async Task UpdateBudgetAsync(BudgetViewModel vm, string userId)
        {
            var budget = await _budgetRepo.GetByIdAsync(vm.Id);
            if (budget == null || budget.UserId != userId) return;

            budget.Name = vm.Name;
            budget.Amount = vm.Amount;
            budget.BudgetType = vm.BudgetType;
            budget.Month = vm.Month;
            budget.Year = vm.Year;
            budget.CategoryId = vm.CategoryId;

            await _budgetRepo.UpdateAsync(budget);
            await _budgetRepo.SaveChangesAsync();
        }

        public async Task DeleteBudgetAsync(int id, string userId)
        {
            var budget = await _budgetRepo.GetByIdAsync(id);
            if (budget == null || budget.UserId != userId) return;
            await _budgetRepo.DeleteAsync(budget);
            await _budgetRepo.SaveChangesAsync();
        }

        public async Task<IEnumerable<BudgetStatusViewModel>> GetBudgetStatusAsync(string userId, int month, int year)
        {
            var budgets = await _budgetRepo.GetByUserIdAsync(userId);
            var result = new List<BudgetStatusViewModel>();

            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1).AddDays(-1);
            var monthExpenses = await _expenseRepo.GetByUserIdAndDateRangeAsync(userId, from, to);

            foreach (var budget in budgets)
            {
                bool appliesToMonth = (!budget.Month.HasValue || budget.Month.Value == month) &&
                                     (!budget.Year.HasValue || budget.Year.Value == year);

                if (!appliesToMonth) continue;

                decimal spent = 0;
                if (budget.CategoryId.HasValue)
                {
                    spent = monthExpenses.Where(e => e.CategoryId == budget.CategoryId.Value).Sum(e => e.Amount);
                }
                else
                {
                    spent = monthExpenses.Sum(e => e.Amount);
                }

                result.Add(new BudgetStatusViewModel { Budget = budget, Spent = spent });
            }

            return result;
        }
    }
}
