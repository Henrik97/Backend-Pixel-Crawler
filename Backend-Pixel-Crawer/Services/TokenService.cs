using Backend_Pixel_Crawler.Interface;
using SharedLibrary;

namespace Backend_Pixel_Crawler.Services
{
    public class TokenService : ITokenService
    {
        IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(UserModel user)
        {
           var _secretKey = _configuration["Jwt:SecretKey"];

            throw new NotImplementedException();
        }
    }
}
