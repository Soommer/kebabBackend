using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;

namespace kebabBackend.Repositories.Cart
{
    public interface ICartRepository
    {
        Task<CartResponse> CreateAsync(CartCreateRequest request);
        Task<IEnumerable<CartResponse>> GetAllAsync();
        Task<CartResponse> AddExistingItemAsync(Guid cartId, Guid cartItemId);
        Task<CartResponse?> GetByIdAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ComleteCardAsync(Guid id);
        Task<IEnumerable<CartResponse>> GetAllNonCompleteAsync();
        Task<int> GetCartItemCountAsync(Guid cartId);
    }
}
