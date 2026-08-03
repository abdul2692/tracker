using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendingTracker.Models;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Controllers
{
    [Authorize]
    public class IncomeController : BaseController
    {
        private readonly IIncomeService _incomeService;

        public IncomeController(IIncomeService incomeService, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _incomeService = incomeService;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null, string? sortBy = null)
        {
            var userId = _userManager.GetUserId(User)!;
            const int pageSize = 10;
            var all = (await _incomeService.GetUserIncomesAsync(userId)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(i =>
                    i.Source.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (i.Description != null && i.Description.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();

            all = sortBy switch
            {
                "amount_asc" => all.OrderBy(i => i.Amount).ToList(),
                "amount_desc" => all.OrderByDescending(i => i.Amount).ToList(),
                "date_asc" => all.OrderBy(i => i.Date).ToList(),
                _ => all.OrderByDescending(i => i.Date).ToList()
            };

            ViewBag.TotalCount = all.Count;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)all.Count / pageSize);
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalIncome = all.Sum(i => i.Amount);

            var paged = all.Skip((page - 1) * pageSize).Take(pageSize);
            return View(paged);
        }

        [HttpGet]
        public IActionResult Create() => View(new IncomeViewModel { Date = DateTime.Today });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IncomeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var userId = _userManager.GetUserId(User)!;
            await _incomeService.CreateIncomeAsync(vm, userId);
            TempData["Success"] = "Income added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var income = await _incomeService.GetIncomeByIdAsync(id, userId);
            if (income == null) return NotFound();

            var vm = new IncomeViewModel
            {
                Id = income.Id,
                Amount = income.Amount,
                Source = income.Source,
                Date = income.Date,
                Description = income.Description
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IncomeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var userId = _userManager.GetUserId(User)!;
            await _incomeService.UpdateIncomeAsync(vm, userId);
            TempData["Success"] = "Income updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var income = await _incomeService.GetIncomeByIdAsync(id, userId);
            if (income == null) return NotFound();
            return View(income);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _incomeService.DeleteIncomeAsync(id, userId);
            TempData["Success"] = "Income deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
