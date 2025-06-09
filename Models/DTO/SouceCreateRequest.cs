using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class SouceCreateRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
    }
}
