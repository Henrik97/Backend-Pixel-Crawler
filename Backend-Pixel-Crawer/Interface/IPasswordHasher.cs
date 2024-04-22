using SharedLibrary;

namespace Backend_Pixel_Crawler.Interface
{
    public interface IPasswordHasher
    {
        PasswordResultModel HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword, byte[] storedSalt);
    }
}
