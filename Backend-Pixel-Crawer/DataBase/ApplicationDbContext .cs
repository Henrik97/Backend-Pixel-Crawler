using Microsoft.EntityFrameworkCore;
using SharedLibrary;

namespace Backend_Pixel_Crawler.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<Player> Player { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) { }

    }
}
