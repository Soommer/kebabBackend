namespace kebabBackend.Models.Domain
{
    public class CartItemExtraIngredient
    {
        public Guid Id { get; set; }

        public Guid CartItemId { get; set; }
        public CartItem CartItem { get; set; }

        public Guid ExtraIngredientId { get; set; }
        public extraIgredients ExtraIngredient { get; set; }
    }
}
