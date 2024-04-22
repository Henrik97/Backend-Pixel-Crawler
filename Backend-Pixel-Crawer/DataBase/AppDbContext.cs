using Microsoft.EntityFrameworkCore;
using SharedLibrary;

namespace Backend_Pixel_Crawler.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
