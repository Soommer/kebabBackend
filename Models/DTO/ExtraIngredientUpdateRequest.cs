using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class ExtraIngredientUpdateRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public float Price { get; set; }
    }

}
