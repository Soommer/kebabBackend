using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class UserCreateRequest
    {
        [Required]
        public string UserEmail { get; set; }
        [Required]
        public string UserPhoneNumber { get; set; }

        [Required]
        public int LoyalityPoints { get; set; } = 0;

        public string Address { get; set; }
    }
}
