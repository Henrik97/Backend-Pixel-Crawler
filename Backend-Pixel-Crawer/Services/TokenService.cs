using Backend_Pixel_Crawler.Interface;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

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
        private RSA LoadPrivateKey()
        {
            string privateKeyPath = _configuration["Jwt:PrivateKeyPath"];
            string privateKeyContent = File.ReadAllText(privateKeyPath).Trim();

         
            string base64PrivateKey = (privateKeyContent).Replace("\n", "").Replace("\r", "").Trim();

            var rsa = RSA.Create();
            byte[] rsaPrivateKeyBytes = Convert.FromBase64String(base64PrivateKey);

            try
            {
                rsa.ImportPkcs8PrivateKey(new ReadOnlySpan<byte>(rsaPrivateKeyBytes), out _);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing private key: {ex.Message}");
                throw; // Rethrow or handle the exception appropriately
            }
            return rsa;
        }

        private RSA LoadPublicKey()
        {
            string publicKeyPath = _configuration["Jwt:PublicKeyPath"];
            string publicKey = File.ReadAllText(publicKeyPath);
            Console.WriteLine("Public Key Content: " + publicKey);
            try
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(publicKey.ToCharArray());
                return rsa;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load public key: " + ex.Message);
                throw;  // Re-throw the exception to handle it further up the stack
            }
        }

        public string GenerateToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            using (RSA rsa = LoadPrivateKey())
            {
                var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = credentials
                };
                return tokenHandler.CreateEncodedJwt(tokenDescriptor);
            }
        }

        public ClaimsPrincipal ValidateToken(string jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            using (RSA rsa = LoadPublicKey())
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                try
                {
                    var principal = tokenHandler.ValidateToken(jwt, validationParameters, out SecurityToken validatedToken);
                    return principal;
                }
                catch
                {
                    // Handle validation failure
                    return null;
                }
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
            if (string.IsNullOrEmpty(token))
                return token;

            // Remove null characters and spaces
            return token.Replace("\0", string.Empty).Replace(" ", string.Empty);
        }
    }
}
