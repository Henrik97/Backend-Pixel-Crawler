using SharedLibrary;
using System.Threading.Tasks;

namespace Backend_Pixel_Crawler.Interface
{
    public interface IUserService
    {
        Task<(bool Success, string ErrorMessage)> CreateUserAsync(CreateUserModel createUser);

        Task<(bool IsAuthenticated, string Token)> AuthenticateUserAsync(string username, string password);

        Task<bool> UsernameExists(string username);

        Task<bool> EmailExists(string email);

        Task<string?> GetUserIdFromEmail(string email);

    }
}
