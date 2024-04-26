using SharedLibrary;
using System.Security.Claims;

namespace Backend_Pixel_Crawler.Interface
{
    public interface ITokenService
    {
        string GenerateToken(UserModel user);
        ClaimsPrincipal ValidateToken(string token);

        bool DoesTokenExist(string userId, string incomingToken);
    }
}
