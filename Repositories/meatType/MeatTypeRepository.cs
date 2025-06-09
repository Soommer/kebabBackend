using kebabBackend.Data;
using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;

namespace kebabBackend.Repositories.meatType
{
    public class MeatTypeRepository : IMeatTypeRepository
    {
        private readonly KebabDbContext _context;

        public MeatTypeRepository(KebabDbContext context)
        {
            this._context = context;
        }

        public async Task<IEnumerable<MeatType>> GetAllAsync()
        {
            return await _context.meatTypes
                .OrderBy(m => m.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MeatType?> GetByIdAsync(Guid id)
        {
            return await _context.meatTypes.FindAsync(id);
        }

        public async Task<MeatType> AddAsync(MeatTypeCreateRequest request)
        {
            var entity = new MeatType
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ExtraPrice = request.ExtraPrice
            };

            await _context.meatTypes.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<bool> UpdateAsync(Guid id, MeatTypeUpdateRequest request)
        {
            var meat = await _context.meatTypes.FindAsync(id);
            if (meat == null) return false;

            meat.Name = request.Name;
            meat.ExtraPrice = request.ExtraPrice;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var meat = await _context.meatTypes.FindAsync(id);
            if (meat == null) return false;

            _context.meatTypes.Remove(meat);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
