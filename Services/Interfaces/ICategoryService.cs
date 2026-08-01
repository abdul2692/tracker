using SpendingTracker.Models;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAvailableForUserAsync(string userId);
        Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId);
        Task<Category?> GetByIdAsync(int id);
        Task CreateCategoryAsync(CategoryViewModel vm, string userId);
        Task UpdateCategoryAsync(CategoryViewModel vm, string userId);
        Task DeleteCategoryAsync(int id, string userId);
    }
}
