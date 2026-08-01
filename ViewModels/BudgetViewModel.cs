using Microsoft.AspNetCore.Mvc.Rendering;
using SpendingTracker.Models;
using System.ComponentModel.DataAnnotations;

namespace SpendingTracker.ViewModels
{
    public class BudgetViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Budget Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Budget Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Budget Type")]
        public BudgetType BudgetType { get; set; } = BudgetType.Monthly;

        [Display(Name = "Month")]
        [Range(1, 12)]
        public int? Month { get; set; }

        [Display(Name = "Year")]
        public int? Year { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        // For dropdowns
        public List<SelectListItem> Categories { get; set; } = new();
    }

    public class BudgetStatusViewModel
    {
        public Budget Budget { get; set; } = null!;
        public decimal Spent { get; set; }
        public decimal Remaining => Budget.Amount - Spent;
        public decimal PercentageUsed => Budget.Amount > 0 ? (Spent / Budget.Amount) * 100 : 0;
        public bool IsOverBudget => Spent > Budget.Amount;
        public bool IsWarning => PercentageUsed >= 80 && !IsOverBudget;
    }
}
