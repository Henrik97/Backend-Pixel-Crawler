using Backend_Pixel_Crawler.Database;
using Backend_Pixel_Crawler.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend_Pixel_Crawler.Services
{
    public class AuthenticationService : IUserAuthenticationService
    {
        private readonly IPasswordHasher _hashPasswordService;
        private readonly AppDbContext _context;

        public AuthenticationService(IPasswordHasher hashPasswordService)
        {
            _hashPasswordService = hashPasswordService;
        }

        public async Task<(bool IsAuthenticated, string Token)> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);

            var isValidPassword = _hashPasswordService.VerifyPassword(password, user.HashedPassword, user.Salt);


            if (isValidPassword)
            {
                generateToken()

                return (true,)
            }
            return await AuthenticateUserAsync(username, password);
        }
    }
}
