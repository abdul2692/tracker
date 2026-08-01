using System.ComponentModel.DataAnnotations;

namespace SpendingTracker.ViewModels
{
    public class IncomeViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Source")]
        public string Source { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
