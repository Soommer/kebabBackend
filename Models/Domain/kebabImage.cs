using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kebabBackend.Models.Domain
{
    public class kebabImage
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [NotMapped]
        public IFormFile File { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        public string FileExtention { get; set; }

        [Required]
        public long FileSizeInBytes { get; set; }

        [Required]
        public string FilePath { get; set; }
    }
}
