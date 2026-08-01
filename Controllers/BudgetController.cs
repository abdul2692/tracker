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
    public class BudgetController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BudgetController(IBudgetService budgetService, ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            _budgetService = budgetService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var now = DateTime.Now;
            var statuses = await _budgetService.GetBudgetStatusAsync(userId, now.Month, now.Year);
            return View(statuses);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User)!;
            var cats = await _categoryService.GetAvailableForUserAsync(userId);
            var vm = new BudgetViewModel
            {
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetViewModel vm)
        {
            var userId = _userManager.GetUserId(User)!;
            if (!ModelState.IsValid)
            {
                var cats = await _categoryService.GetAvailableForUserAsync(userId);
                vm.Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
                return View(vm);
            }
            await _budgetService.CreateBudgetAsync(vm, userId);
            TempData["Success"] = "Budget created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var budget = await _budgetService.GetByIdAsync(id, userId);
            if (budget == null) return NotFound();

            var cats = await _categoryService.GetAvailableForUserAsync(userId);
            var vm = new BudgetViewModel
            {
                Id = budget.Id,
                Name = budget.Name,
                Amount = budget.Amount,
                BudgetType = budget.BudgetType,
                Month = budget.Month,
                Year = budget.Year,
                CategoryId = budget.CategoryId,
                Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BudgetViewModel vm)
        {
            var userId = _userManager.GetUserId(User)!;
            if (!ModelState.IsValid)
            {
                var cats = await _categoryService.GetAvailableForUserAsync(userId);
                vm.Categories = cats.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
                return View(vm);
            }
            await _budgetService.UpdateBudgetAsync(vm, userId);
            TempData["Success"] = "Budget updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var budget = await _budgetService.GetByIdAsync(id, userId);
            if (budget == null) return NotFound();
            return View(budget);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _budgetService.DeleteBudgetAsync(id, userId);
            TempData["Success"] = "Budget deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
