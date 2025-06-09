using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class MenuItemUpdateRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public float BasePrice { get; set; }

        public IFormFile? NewImage { get; set; }  
    }
}
