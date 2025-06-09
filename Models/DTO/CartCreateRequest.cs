using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.DTO
{
    public class CartCreateRequest
    {

        [Required]
        public List<Guid> CartItemIds { get; set; } = new();
    }
}
