using SpendingTracker.Models;

namespace SpendingTracker.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAvailableForUserAsync(string userId);
        Task<IEnumerable<Category>> GetDefaultCategoriesAsync();
        Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId);
    }
}
