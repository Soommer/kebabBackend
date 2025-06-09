using System.ComponentModel.DataAnnotations;


namespace kebabBackend.Models.Domain
{
    public class CartItem
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Size { get; set; }
        [Required]
        public Guid MenuItemId { get; set; }
        [Required] 
        public menuItem MenuItem { get; set; }

        [Required]
        public Guid MeatTypeId { get; set; }
        [Required] 
        public MeatType MeatType { get; set; }

        [Required]
        public Guid SouceId { get; set; }
        [Required]
        public Souce Souce { get; set; }

        public ICollection<CartItemExtraIngredient> ExtraIngredientsLinks { get; set; } = new List<CartItemExtraIngredient>();

        [Required]
        public float TotalPrice { get; set; } 
        [Required]
        public DateTime CreatedAt { get; set; }

        public Guid? CartId { get; set; }

        public Cart Cart { get; set; } = null!;
    }
}
