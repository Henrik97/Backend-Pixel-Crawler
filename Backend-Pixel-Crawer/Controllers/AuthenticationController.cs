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
        private readonly IUserService  _userService;

            public AuthenticationController(ApplicationDbContext context, IPasswordHasher passwordHasher, IUserAuthenticationService userAuthentication, IUserService userService )
            {
                _context = context;
                _passwordHasher = passwordHasher;
                _userAuthenticationService = userAuthentication;
                _userService = userService;
                
            
            }



            [HttpPost("register")]
            public async Task<ActionResult> Register([FromBody] CreateUserModel createUser)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

            var wasUserCreated = await _userService.CreateUserAsync(createUser);


            if (wasUserCreated.Success)
            {
                return Ok("User registered successfully");
            };


            return BadRequest(wasUserCreated.ErrorMessage);


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
