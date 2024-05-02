using Backend_Pixel_Crawler.Database;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using Microsoft.EntityFrameworkCore;
using Backend_Pixel_Crawler.Services;
using Backend_Pixel_Crawler.Interface;
using Newtonsoft.Json.Linq;

namespace Backend_Pixel_Crawler.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase

    {

        private readonly IPasswordHasher _passwordHasher;
        private readonly ApplicationDbContext _context;
        private readonly IUserAuthenticationService _userAuthenticationService;

            public AuthenticationController(ApplicationDbContext context, IPasswordHasher passwordHasher, IUserAuthenticationService userAuthentication )
            {
                _context = context;
                _passwordHasher = passwordHasher;
                _userAuthenticationService = userAuthentication;
                
            
            }



            [HttpPost("register")]
            public async Task<ActionResult> Register([FromBody] CreateUserModel createUser)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var hashPassword = _passwordHasher.HashPassword(createUser.Password);

                var user = new UserModel
                {
                    Name = createUser.Name,
                    Username = createUser.Username,
                    Email = createUser.Email,
                    Salt = hashPassword.Salt,
                    HashedPassword = hashPassword.HashPassword

                };

                _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User registered successfully");


        }

            [HttpPost("login")]
            public async Task<ActionResult> Login([FromBody] LoginModel login)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

            var authenticationState = await _userAuthenticationService.AuthenticateUserAsync(login.Username, login.Password);

            if (authenticationState.IsAuthenticated)
            {
                    
                   
                return Ok(new { authenticationState.Token });
            }

            return BadRequest("Username or Password incorrect");
            }


} }
