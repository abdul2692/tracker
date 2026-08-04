using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Controllers
{
    [Authorize]
    public class ExpenseController : BaseController
    {
        private readonly IExpenseService _expenseService;
        private readonly ICategoryService _categoryService;

        public ExpenseController(IExpenseService expenseService, ICategoryService categoryService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? sortBy = null, int? categoryId = null, int? month = null, int? year = null)
        {
            var userId = _userManager.GetUserId(User)!;
            const int pageSize = 10;
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            var all = (await _expenseService.GetUserExpensesAsync(userId)).ToList();

            // Filter strictly by month and year
            all = all.Where(e => e.Date.Month == selectedMonth && e.Date.Year == selectedYear).ToList();

            if (categoryId.HasValue)
                all = all.Where(e => e.CategoryId == categoryId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(e =>
                    (e.Description != null && e.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    e.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            all = sortBy switch
            {
                "amount_asc" => all.OrderBy(e => e.Amount).ToList(),
                "amount_desc" => all.OrderByDescending(e => e.Amount).ToList(),
                "date_asc" => all.OrderBy(e => e.Date).ToList(),
                "category" => all.OrderBy(e => e.Category.Name).ToList(),
                _ => all.OrderByDescending(e => e.Date).ToList()
            };

            ViewBag.TotalCount = all.Count;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)all.Count / pageSize);
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.CategoryId = categoryId;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.TotalExpenses = all.Sum(e => e.Amount);
            ViewBag.Categories = await _categoryService.GetAvailableForUserAsync(userId);

            var paged = all.Skip((page - 1) * pageSize).Take(pageSize);
            return View(paged);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User)!;
            var categories = await _categoryService.GetAvailableForUserAsync(userId);
            var vm = new ExpenseViewModel
            {
                Date = DateTime.Today,
                Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseViewModel vm)
        {
            var userId = _userManager.GetUserId(User)!;
            if (!ModelState.IsValid)
            {
                var cats = await _categoryService.GetAvailableForUserAsync(userId);
                vm.Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
                return View(vm);
            }
            await _expenseService.CreateExpenseAsync(vm, userId);
            TempData["Success"] = "Expense added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var expense = await _expenseService.GetExpenseByIdAsync(id, userId);
            if (expense == null) return NotFound();

            var categories = await _categoryService.GetAvailableForUserAsync(userId);
            var vm = new ExpenseViewModel
            {
                Id = expense.Id,
                Amount = expense.Amount,
                CategoryId = expense.CategoryId,
                Date = expense.Date,
                Description = expense.Description,
                PaymentMethod = expense.PaymentMethod,
                Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExpenseViewModel vm)
        {
            var userId = _userManager.GetUserId(User)!;
            if (!ModelState.IsValid)
            {
                var cats = await _categoryService.GetAvailableForUserAsync(userId);
                vm.Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
                return View(vm);
            }
            await _expenseService.UpdateExpenseAsync(vm, userId);
            TempData["Success"] = "Expense updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var expense = await _expenseService.GetExpenseByIdAsync(id, userId);
            if (expense == null) return NotFound();
            return View(expense);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _expenseService.DeleteExpenseAsync(id, userId);
            TempData["Success"] = "Expense deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
