using System.ComponentModel.DataAnnotations;

namespace s3_file_manager_backend.Models
{
    public class StoredFile
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public string FileLink { get; set; }
    }
}
