using kebabBackend.Data;
using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;

namespace kebabBackend.Repositories.Souces
{
    public class SouceRepository : ISouceRepository
    {
        private readonly KebabDbContext _context;

        public SouceRepository(KebabDbContext context)
        {
            this._context = context;
        }

        public async Task<IEnumerable<Souce>> GetAllAsync()
        {
            return await _context.souces
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Souce?> GetByIdAsync(Guid id)
        {
            return await _context.souces.FindAsync(id);
        }

        public async Task<Souce> AddAsync(SouceCreateRequest request)
        {
            var entity = new Souce
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            await _context.souces.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<bool> UpdateAsync(Guid id, SouceUpdateRequest request)
        {
            var entity = await _context.souces.FindAsync(id);
            if (entity == null) return false;

            entity.Name = request.Name;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.souces.FindAsync(id);
            if (entity == null) return false;

            _context.souces.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
