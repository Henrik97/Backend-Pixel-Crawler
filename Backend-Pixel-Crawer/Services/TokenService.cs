using Backend_Pixel_Crawler.Interface;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;

namespace Backend_Pixel_Crawler.Services
{
    public class TokenService : ITokenService
    {
        IConfiguration _configuration;
        ITokenCacheService _tokenCacheService;

        private readonly string _secretKey;

        public TokenService(IConfiguration configuration, ITokenCacheService cacheService)
        {
            _configuration = configuration;
            _tokenCacheService = cacheService;
            //_secretKey = Environment.GetEnvironmentVariable("JWT_KEY")
            if (_secretKey == null)
            {
                // Handle the case where the environmental variable is not set
                // This could include logging an error message or throwing an exception
                Console.WriteLine("JWT_KEY environmental variable is not set.");
                // Optionally, throw an exception to stop execution
                // throw new Exception("JWT_KEY environmental variable is not set.");
            }
            else
            {
                Console.WriteLine("JWT Key: " + _secretKey);
            }
        }

        public string GenerateToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = credentials,
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {

           var cleanToken =  SanitizeToken(token);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
            };

            try
            {
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(cleanToken, validationParameters, out validatedToken);
                Console.WriteLine("Token is valid.");
                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DoesTokenExist(string userId, string incomingToken)
        {
            string storedToken = await _tokenCacheService.GetTokenAsync(userId);

            if (string.IsNullOrEmpty(storedToken))
            {
                return false; // token was not found
            }

            return storedToken == incomingToken;
        }

        public string SanitizeToken(string token)
        {
            return new string(token.Where(c => !char.IsControl(c) || c == '\t' || c == '\n' || c == '\r').ToArray());
        }
    }
}
