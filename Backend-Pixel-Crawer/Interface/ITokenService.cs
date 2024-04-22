using SharedLibrary;

namespace Backend_Pixel_Crawler.Interface
{
    public interface ITokenService
    {
        string GenerateToken(UserModel user);
        bool ValidateToken(string token);
    }
}
