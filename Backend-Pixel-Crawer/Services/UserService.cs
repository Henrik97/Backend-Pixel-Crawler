using Backend_Pixel_Crawler.Interface;
using Microsoft.AspNetCore.Authentication;
using Backend_Pixel_Crawler.Database;
using SharedLibrary;
using Microsoft.EntityFrameworkCore;

namespace Backend_Pixel_Crawler.Services
{
    public class UserService : IUserService
    {
          ApplicationDbContext _context;
          IPasswordHasher _passwordHasher;
          IUserAuthenticationService _authenticationService;

        public UserService(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IUserAuthenticationService authenticationService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _authenticationService = authenticationService;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateUserAsync(CreateUserModel createUser)
        {
            if (await UsernameExists(createUser.Username))
            {
                return (false, "User name is taken");
            }
            if (await EmailExists(createUser.Email))
            {
                return (false, "Email is aldready in use");
            }

            var user = new UserModel
            {
                Id = Guid.NewGuid(),
                Name = createUser.Name,
                Username = createUser.Username,
                Email = createUser.Email,
                Salt = _passwordHasher.HashPassword(createUser.Password).Salt,
                HashedPassword = _passwordHasher.HashPassword(createUser.Password).HashPassword,
                Password = createUser.Password,

            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Succes");
        }

        public async Task<(bool IsAuthenticated, string Token)> AuthenticateUserAsync(string username, string password)
        {
            return await _authenticationService.AuthenticateUserAsync(username, password);
        }


        public async Task<bool> UsernameExists(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

    }
}
