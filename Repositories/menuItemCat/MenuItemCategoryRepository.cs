using kebabBackend.Data;
using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace kebabBackend.Repositories.menuItemCat
{
    public class MenuItemCategoryRepository : IMenuItemCategoryRepository
    {
        private readonly KebabDbContext _context;

        public MenuItemCategoryRepository(KebabDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MenuItemCategoryResponse>> GetAllAsync()
        {
            return await _context.menuItemCategories
                .OrderBy(c => c.Name)
                .Select(c => new MenuItemCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<MenuItemCategoryResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _context.menuItemCategories.FindAsync(id);
            if (entity == null) return null;

            return new MenuItemCategoryResponse
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public async Task<MenuItemCategoryResponse> CreateAsync(MenuItemCategoryCreateRequest request)
        {
            var entity = new menuItemCategory
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            await _context.menuItemCategories.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new MenuItemCategoryResponse
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public async Task<bool> UpdateAsync(Guid id, MenuItemCategoryCreateRequest request)
        {
            var entity = await _context.menuItemCategories.FindAsync(id);
            if (entity == null) return false;

            entity.Name = request.Name;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.menuItemCategories.FindAsync(id);
            if (entity == null) return false;

            _context.menuItemCategories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
