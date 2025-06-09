using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;

namespace kebabBackend.Repositories.MenuItem
{
    public interface IMenuItemRepository
    {
        Task<menuItem> AddMenuItemAsync(menuItem menuItem, IFormFile imageFile);
        Task<IEnumerable<MenuItemReturn>> GetAllMenuItemsAsync(Guid? categoryId = null);
        Task<bool> DeleteMenuItemAsync(Guid id);
        Task<bool> UpdateMenuItemAsync(Guid id, MenuItemUpdateRequest request);
        Task<bool> UpdateAsyncNoPicture(Guid id, MenuItemUpdateNoPicture request);
    }
}
