using Microsoft.AspNetCore.Mvc;
using SharedLibrary;

namespace Backend_Pixel_Crawer.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class PlayerController : ControllerBase
    {
        [HttpGet]
        public Player get(  )
        {
            var player = new Player();

            return player;
        }
    }
}
