using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class UserUpdateRequest
    {
        [Required]
        public string UserEmail { get; set; }
        [Required]
        public string UserPhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
