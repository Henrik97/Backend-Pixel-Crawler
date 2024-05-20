using Backend_Pixel_Crawler.Database;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;

namespace Backend_Pixel_Crawler.Services
{
    public class PlayerService
    {
        ApplicationDbContext _context;
        public PlayerService(ApplicationDbContext context) {
        
            _context = context;
        }


        public async Task<Player> FindPlayerInDbByUserID(string userId) 
        {
            Player player = await _context.Player.FirstOrDefaultAsync(u => u.UserID == userId);
            return player;
        }

        public async Task AddPlayerToDb(Player player)
        {
            _context.Player.Add(player);
            await _context.SaveChangesAsync();
        }
    }
}
