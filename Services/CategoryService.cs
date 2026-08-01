using SpendingTracker.Models;
using SpendingTracker.Repositories.Interfaces;
using SpendingTracker.Services.Interfaces;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Category>> GetAvailableForUserAsync(string userId)
            => await _repo.GetAvailableForUserAsync(userId);

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId)
            => await _repo.GetUserCategoriesAsync(userId);

        public async Task<Category?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task CreateCategoryAsync(CategoryViewModel vm, string userId)
        {
            var cat = new Category
            {
                Name = vm.Name,
                Icon = vm.Icon,
                Color = vm.Color,
                IsDefault = false,
                UserId = userId
            };
            await _repo.AddAsync(cat);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(CategoryViewModel vm, string userId)
        {
            var cat = await _repo.GetByIdAsync(vm.Id);
            if (cat == null || cat.UserId != userId) return;

            cat.Name = vm.Name;
            cat.Icon = vm.Icon;
            cat.Color = vm.Color;

            await _repo.UpdateAsync(cat);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id, string userId)
        {
            var cat = await _repo.GetByIdAsync(id);
            if (cat == null || cat.UserId != userId) return;
            await _repo.DeleteAsync(cat);
            await _repo.SaveChangesAsync();
        }
    }
}
