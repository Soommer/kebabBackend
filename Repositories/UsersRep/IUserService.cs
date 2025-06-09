using kebabBackend.Models.Domain;
using kebabBackend.Models.DTO;


namespace kebabBackend.Repositories.UsersRep
{
    public interface IUserService
    {
        Task<bool>  EmailExistsAsync(string email);
        Task<User> RegisterAsync(RegisterRequest request);
    }
}
