using System.ComponentModel.DataAnnotations;


namespace kebabBackend.Models.Domain
{
    public class extraIgredients
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        public float price { get; set; }
    }
}
