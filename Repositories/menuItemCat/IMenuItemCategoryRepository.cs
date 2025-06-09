using kebabBackend.Models.DTO;

namespace kebabBackend.Repositories.menuItemCat
{
    public interface IMenuItemCategoryRepository
    {
        Task<IEnumerable<MenuItemCategoryResponse>> GetAllAsync();
        Task<MenuItemCategoryResponse?> GetByIdAsync(Guid id);
        Task<MenuItemCategoryResponse> CreateAsync(MenuItemCategoryCreateRequest request);
        Task<bool> UpdateAsync(Guid id, MenuItemCategoryCreateRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
