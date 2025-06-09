using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;

namespace kebabBackend.Repositories.meatType
{
    public interface IMeatTypeRepository
    {
        Task<IEnumerable<MeatType>> GetAllAsync();
        Task<MeatType?> GetByIdAsync(Guid id);
        Task<MeatType> AddAsync(MeatTypeCreateRequest request);
        Task<bool> UpdateAsync(Guid id, MeatTypeUpdateRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
