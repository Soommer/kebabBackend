using System.ComponentModel.DataAnnotations;


namespace kebabBackend.Models.Domain
{
    public class MeatType
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
        [Required]
        public float ExtraPrice { get; set; }
    }
}
