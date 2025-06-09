using System.ComponentModel.DataAnnotations;


namespace kebabBackend.Models.Domain
{
    public class paydOrder
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid CartId { get; set; }
        [Required]
        public Cart Cart { get; set; }

        [Required]
        public string address { get; set; }
        [Required]
        public string Status {  get; set; }
        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
