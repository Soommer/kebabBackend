using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class RegisterRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
