namespace kebabBackend.Models.DTO
{
    public class CartItemResponse
    {
        public Guid Id { get; set; }
        public string Size { get; set; }
        public string MenuItemName { get; set; }
        public string MeatTypeName { get; set; }
        public string SouceName { get; set; }
        public float TotalPrice { get; set; }
        public List<string> ExtraIngredientNames { get; set; } = new();
    }

}
