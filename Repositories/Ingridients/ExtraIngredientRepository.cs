using kebabBackend.Data;
using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;

namespace kebabBackend.Repositories.Ingridients
{
    public class ExtraIngredientRepository : IExtraIngredientRepository
    {
        private readonly KebabDbContext _context;

        public ExtraIngredientRepository(KebabDbContext context)
        {
            this._context = context;
        }

        public async Task<IEnumerable<extraIgredients>> GetAllAsync()
        {
            return await _context.extraIgredients
                .OrderBy(e => e.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<extraIgredients?> GetByIdAsync(Guid id)
        {
            return await _context.extraIgredients.FindAsync(id);
        }

        public async Task<extraIgredients> AddAsync(ExtraIngredientCreateRequest request)
        {
            var entity = new extraIgredients
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                price = request.Price
            };

            await _context.extraIgredients.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(Guid id, ExtraIngredientUpdateRequest request)
        {
            var ingredient = await _context.extraIgredients.FindAsync(id);
            if (ingredient == null) return false;

            ingredient.Name = request.Name;
            ingredient.Description = request.Description;
            ingredient.price = request.Price;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var ingredient = await _context.extraIgredients.FindAsync(id);
            if (ingredient == null) return false;

            _context.extraIgredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
