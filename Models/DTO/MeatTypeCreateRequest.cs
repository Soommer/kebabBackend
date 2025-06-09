using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class MeatTypeCreateRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public float ExtraPrice { get; set; }
    }
}
