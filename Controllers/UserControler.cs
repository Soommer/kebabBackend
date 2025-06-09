using kebabBackend.Data;
using kebabBackend.Models.DTO;
using kebabBackend.Repositories.UsersRep;
using kebabBackend.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;

namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly KebabDbContext _context;
        private readonly TokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthController> _logger;


        public AuthController(IUserService userService, KebabDbContext context, TokenService tokenService, JwtSettings jwtSettings,  ILogger<AuthController> logger)
        {
            _logger = logger;
            _userService = userService;
            _context = context;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _userService.EmailExistsAsync(request.Email))
                return BadRequest("Email already registered.");

            var user = await _userService.RegisterAsync(request);
            return Ok(new { user.Id, user.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.user.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Nieprawidłowe dane logowania.");

            var tokens = _tokenService.GenerateToken(user);
            user.RefreshToken = tokens.RefreshToken;
            user.RefreshTokenExpiryTime = tokens.ExpiresAt.AddDays(_jwtSettings.RefreshTokenExpiresInDays);
            await _context.SaveChangesAsync();

            return Ok(tokens);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            var user = await _context.user.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized("Nieprawidłowy lub wygasły token odświeżania.");

            var tokens = _tokenService.GenerateToken(user);

            user.RefreshToken = tokens.RefreshToken;
            user.RefreshTokenExpiryTime = tokens.ExpiresAt.AddDays(_jwtSettings.RefreshTokenExpiresInDays);

            await _context.SaveChangesAsync();

            return Ok(tokens);
        }

    }
}
