using System.ComponentModel.DataAnnotations;


namespace kebabBackend.Models.Domain
{
    public class Cart
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public float Total { get; set; } 
        [Required]  
        public DateTime CreatedAt { get; set; }
        [Required]
        public bool IsPaid { get; set; } = false;
        [Required]
        public bool IsActive { get; set; } = true;
        [Required]
        public bool IsFinished { get; set; } = false;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        public bool IsProcessed { get; set; } = false;


        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
