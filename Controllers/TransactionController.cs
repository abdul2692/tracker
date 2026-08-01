using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly IIncomeService _incomeService;
        private readonly IExpenseService _expenseService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionController(IIncomeService incomeService, IExpenseService expenseService,
            ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            _incomeService = incomeService;
            _expenseService = expenseService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? sortBy = null,
            DateTime? fromDate = null, DateTime? toDate = null, int? categoryId = null, string? type = null)
        {
            var userId = _userManager.GetUserId(User)!;
            const int pageSize = 15;

            var incomes = (await _incomeService.GetUserIncomesAsync(userId))
                .Select(i => new TransactionItem
                {
                    Id = i.Id, Type = "Income", Amount = i.Amount,
                    Description = i.Description ?? i.Source, Date = i.Date,
                    Source = i.Source
                });

            var expenses = (await _expenseService.GetUserExpensesAsync(userId))
                .Select(e => new TransactionItem
                {
                    Id = e.Id, Type = "Expense", Amount = e.Amount,
                    Description = e.Description ?? e.Category.Name, Date = e.Date,
                    CategoryName = e.Category.Name, CategoryColor = e.Category.Color,
                    CategoryIcon = e.Category.Icon,
                    PaymentMethod = e.PaymentMethod.ToString()
                });

            IEnumerable<TransactionItem> all = incomes.Concat(expenses);

            if (type == "Income") all = all.Where(t => t.Type == "Income");
            else if (type == "Expense") all = all.Where(t => t.Type == "Expense");

            if (fromDate.HasValue) all = all.Where(t => t.Date >= fromDate.Value);
            if (toDate.HasValue) all = all.Where(t => t.Date <= toDate.Value);
            if (categoryId.HasValue) all = all.Where(t => t.Type == "Expense"); // handled below

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(t =>
                    t.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (t.CategoryName != null && t.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Source != null && t.Source.Contains(search, StringComparison.OrdinalIgnoreCase)));

            all = sortBy switch
            {
                "amount_asc" => all.OrderBy(t => t.Amount),
                "amount_desc" => all.OrderByDescending(t => t.Amount),
                "date_asc" => all.OrderBy(t => t.Date),
                "type" => all.OrderBy(t => t.Type),
                _ => all.OrderByDescending(t => t.Date)
            };

            var totalCount = all.Count();
            var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var cats = await _categoryService.GetAvailableForUserAsync(userId);

            var vm = new TransactionListViewModel
            {
                Transactions = paged,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                FromDate = fromDate,
                ToDate = toDate,
                CategoryId = categoryId,
                TransactionType = type,
                Categories = cats.ToList()
            };

            return View(vm);
        }
    }
}
