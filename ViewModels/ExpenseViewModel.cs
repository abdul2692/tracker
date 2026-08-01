using Microsoft.AspNetCore.Mvc.Rendering;
using SpendingTracker.Models;
using System.ComponentModel.DataAnnotations;

namespace SpendingTracker.ViewModels
{
    public class ExpenseViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        // For dropdown
        public List<SelectListItem> Categories { get; set; } = new();
    }
}
