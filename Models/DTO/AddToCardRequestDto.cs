using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class AddToCardRequestDto
    {
        public Guid? CartId { get; set; }

        [Required]
        public Guid MenuItemId { get; set; }

        [Required]
        public Guid MeatTypeId { get; set; }

        [Required]
        public Guid SouceId { get; set; }
        [Required]
        public string Size { get; set; }   
        public List<Guid>? ExtraIngredientIds { get; set; } = new();
    }
}
