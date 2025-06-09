using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class MenuItemCreateRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public float BasePrice { get; set; }

        [Required]
        public IFormFile Image { get; set; } = null!;

        [Required]
        public Guid CategoryId { get; set; }
    }
}
