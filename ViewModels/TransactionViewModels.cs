using SpendingTracker.Models;

namespace SpendingTracker.ViewModels
{
    public class TransactionListViewModel
    {
        public IEnumerable<TransactionItem> Transactions { get; set; } = new List<TransactionItem>();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CategoryId { get; set; }
        public string? TransactionType { get; set; } // "income" | "expense" | null

        public List<Category> Categories { get; set; } = new();
    }

    public class TransactionItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }
        public string? CategoryIcon { get; set; }
        public string? Source { get; set; } // for income
        public string? PaymentMethod { get; set; } // for expense
    }
}
