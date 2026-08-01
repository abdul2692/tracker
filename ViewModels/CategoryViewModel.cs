using System.ComponentModel.DataAnnotations;

namespace SpendingTracker.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Icon (Bootstrap Icon class)")]
        [StringLength(50)]
        public string Icon { get; set; } = "bi-tag-fill";

        [Display(Name = "Color")]
        [StringLength(20)]
        public string Color { get; set; } = "#6c757d";
    }
}
