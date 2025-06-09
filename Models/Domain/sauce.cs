using System.ComponentModel.DataAnnotations;

namespace kebabBackend.Models.Domain
{
    public class Souce
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }
    }
}
