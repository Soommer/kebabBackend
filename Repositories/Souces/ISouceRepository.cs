using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;

namespace kebabBackend.Repositories.Souces
{
    public interface ISouceRepository
    {
        Task<IEnumerable<Souce>> GetAllAsync();
        Task<Souce?> GetByIdAsync(Guid id);
        Task<Souce> AddAsync(SouceCreateRequest request);
        Task<bool> UpdateAsync(Guid id, SouceUpdateRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
