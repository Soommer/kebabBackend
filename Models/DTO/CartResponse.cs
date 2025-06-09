namespace kebabBackend.Models.DTO
{

    public class CartItemInCartResponse
    {
        public Guid Id { get; set; }
        public string MenuItemName { get; set; }
        public string MeatName { get; set; }
        public string SouceName { get; set; }
        public List<string> ExtraNames { get; set; } = new();
        public string Size { get; set; }
        public float TotalPrice { get; set; }
    }
    public class CartResponse
    {
        public Guid Id { get; set; }
        public float Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFinished { get; set; }
        public List<CartItemInCartResponse> CartItems { get; set; } = new();
    }
}
