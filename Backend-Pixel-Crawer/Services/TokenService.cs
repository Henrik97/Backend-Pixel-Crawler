﻿using Backend_Pixel_Crawler.Interface;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend_Pixel_Crawler.Services
{
    public class TokenService : ITokenService
    {
        IConfiguration _configuration;
        ITokenCacheService _tokenCacheService;

        public TokenService(IConfiguration configuration, ITokenCacheService cacheService)
        {
            _configuration = configuration;
            _tokenCacheService = cacheService;

        }
        public string GenerateToken(UserModel user)
        {
           var _secretKey = _configuration["Jwt:Key"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); ;
        }

        public ClaimsPrincipal ValidateToken(string token)
        {

            string _secretKey = _configuration["Jwt:Key"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
            };

            Console.WriteLine(token);


            try
            {
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return principal; 
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DoesTokenExist(string userId, string incomingToken)
        {
            string storedToken = await _tokenCacheService.GetTokenAsync(userId);

            if (string.IsNullOrEmpty(storedToken))
            { 
                return false; // token blev ikke fundet
            }

            return storedToken == incomingToken;

        }

    }
}
