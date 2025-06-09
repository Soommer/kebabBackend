using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;

namespace kebabBackend.Repositories.CartItems
{
    public interface ICartItemRepository
    {
        Task<CartItem> CreateAsync(CartItemCreateRequest request);
        Task<CartItem?> GetByIdAsync(Guid id);
        Task<IEnumerable<CartItem>> GetAllAsync();
        Task<bool> DeleteAsync(Guid id);
        Task<CartItemResponse?> GetResponseAsync(Guid id);
        Task<IEnumerable<CartItemResponse>> GetAllResponsesAsync();
    }
}
