using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class SouceUpdateRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
    }
}
