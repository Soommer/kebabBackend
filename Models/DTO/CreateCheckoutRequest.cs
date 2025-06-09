namespace kebabBackend.Models.DTO
{
    public class CreateCheckoutRequest
    {
        public string Address { get; set; }
        public string CartId { get; set; } 
        public string Email { get; set; }
    }

    public class CheckoutItem
    {
        public string Name { get; set; } = string.Empty;
        public float Amount { get; set; }
        public int Quantity { get; set; }
    }
}
