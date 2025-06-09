using kebabBackend.Data;
using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BCrypt;

namespace kebabBackend.Repositories.UsersRep
{
    public class UserService : IUserService
    {
        private readonly KebabDbContext _context;

        public UserService(KebabDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EmailExistsAsync(string email) =>
            await _context.user.AnyAsync(u => u.Email == email);

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.user.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

    }
}
