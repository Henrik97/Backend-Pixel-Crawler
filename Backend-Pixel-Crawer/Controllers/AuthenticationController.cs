using Backend_Pixel_Crawler.Database;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using Microsoft.EntityFrameworkCore;
using Backend_Pixel_Crawler.Services;
using Backend_Pixel_Crawler.Interface;

namespace Backend_Pixel_Crawler.Controllers
{


    [ApiController]
    [Route("[controller]")]

    public class AuthenticationController : ControllerBase

    {

        private readonly IPasswordHasher _passwordHasher;
            private readonly AppDbContext _context;

            public AuthenticationController(AppDbContext context, IPasswordHasher passwordHasher )
            {
                _context = context;
                _passwordHasher = passwordHasher;
            }



            [HttpPost("register")]
            public async Task<ActionResult> Register([FromBody] CreateUserModel createUser)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                var user = new UserModel
                {
                    Name = createUser.Name,
                    Username = createUser.Username,
                    Email = createUser.Email,
                    Salt = _passwordHasher.HashPassword(createUser.Password).Salt,
                    HashedPassword = _passwordHasher.HashPassword(createUser.Password).HashPassword,

                };

                _context.Users.Add(user);
            return Ok("User registered successfully");


        }

            [HttpPost("login")]
            public async Task<ActionResult> Login([FromBody] LoginModel login)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == login.Username);

                var isValidPassword = _passwordHasher.VerifyPassword(login.Password, user.HashedPassword, user.Salt);

            if (isValidPassword)
            {

            }
            }


} }
