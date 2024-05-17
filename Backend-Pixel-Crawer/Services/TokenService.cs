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
           /* if (_secretKey == null)
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
            }*/
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
            Console.WriteLine(token);

 
            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("JWT key is not configured properly.");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                Console.WriteLine("Token is valid.");
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                // Handle the exception based on your specific needs, e.g., logging, throwing a more specific exception, etc.
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return null; // or throw;
            }
            catch (Exception ex)
            {
                // General exception handling, e.g., log and rethrow or return a custom result
                Console.WriteLine($"An error occurred while validating the token: {ex.Message}");
                return null; // or throw;
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
    }
}
