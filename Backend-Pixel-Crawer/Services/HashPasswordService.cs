using Backend_Pixel_Crawler.Interface;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using SharedLibrary;
using System.Collections.Generic;

namespace Backend_Pixel_Crawler.Services
{
    public class HashPasswordService : IPasswordHasher
    {

        public PasswordResultModel HashPassword(string password) {

            // fra .net documentation
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));


            return new PasswordResultModel
        {
            HashPassword = hashedPassword,
            Salt = salt
        };
    }

        public bool VerifyPassword(string password, string storedHashPassword, byte[] storedSalt)
        {
         string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
         password: password,
         salt: storedSalt,
         prf: KeyDerivationPrf.HMACSHA256,
         iterationCount: 100000,
         numBytesRequested: 256 / 8));

            return hashedPassword == storedHashPassword;
        }
    }
}
