using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;

namespace kebabBackend.Repositories.Ingridients
{
    public interface IExtraIngredientRepository
    {
        Task<IEnumerable<extraIgredients>> GetAllAsync();
        Task<extraIgredients?> GetByIdAsync(Guid id);
        Task<extraIgredients> AddAsync(ExtraIngredientCreateRequest request);
        Task<bool> UpdateAsync(Guid id, ExtraIngredientUpdateRequest request);
        Task<bool> DeleteAsync(Guid id);
    }

}
