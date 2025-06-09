using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kebabBackend.Models.Domain
{
    public class menuItem
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public float BasePrice { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
        public menuItemCategory Category { get; set; }

        [Required]
        public Guid ImageId { get; set; }
        public kebabImage Image { get; set; }



    }
}
