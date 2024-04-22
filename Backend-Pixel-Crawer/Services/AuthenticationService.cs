using Backend_Pixel_Crawler.Database;
using Backend_Pixel_Crawler.Interface;
using Backend_Pixel_Crawler.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend_Pixel_Crawler.Services
{
    public class AuthenticationService : IUserAuthenticationService
    {
        IPasswordHasher _hashPasswordService;
        ApplicationDbContext _context;
        ITokenService _tokenService;
        ITokenCacheService _tokenCacheService;

        public AuthenticationService(IPasswordHasher hashPasswordService, ITokenService tokenService, ApplicationDbContext context, ITokenCacheService tokenCacheService)
        {
            _hashPasswordService = hashPasswordService;
            _tokenService = tokenService;
            _context = context;
            _tokenCacheService = tokenCacheService;
        }

        public async Task<(bool IsAuthenticated, string Token)> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);


            if (user != null)
            {
                var isValidPassword = _hashPasswordService.VerifyPassword(password, user.HashedPassword, user.Salt);
                if (isValidPassword)
                {
                    var token = _tokenService.GenerateToken(user);

                    await _tokenCacheService.SetTokenAsync(token, user.Id, TimeSpan.FromHours(1));

                    return (true, token);
                }
            };
            return (false, null); 
        }
    }
}
