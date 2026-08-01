using Microsoft.EntityFrameworkCore;
using SpendingTracker.Data;
using SpendingTracker.Models;
using SpendingTracker.Repositories.Interfaces;

namespace SpendingTracker.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetAvailableForUserAsync(string userId)
            => await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task<IEnumerable<Category>> GetDefaultCategoriesAsync()
            => await _context.Categories
                .Where(c => c.IsDefault)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId)
            => await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
    }
}
