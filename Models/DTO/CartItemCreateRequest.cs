using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class CartItemCreateRequest
    {
        [Required]
        public string Size { get; set; }

        [Required]
        public Guid MenuItemId { get; set; }

        [Required]
        public Guid MeatTypeId { get; set; }

        [Required]
        public Guid SouceId { get; set; }

        public List<Guid> ExtraIngredientIds { get; set; } = new();
    }

}
