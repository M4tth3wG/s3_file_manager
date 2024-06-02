using Microsoft.EntityFrameworkCore;
using System.Xml;
using s3_file_manager_backend.Models;

namespace s3_file_manager_backend.Data
{
    public class FileDbContext(DbContextOptions<FileDbContext> options) : DbContext(options)
    {
        public DbSet<StoredFile> Files { get; set; }
    }
}
