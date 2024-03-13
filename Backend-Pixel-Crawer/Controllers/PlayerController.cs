using Microsoft.AspNetCore.Mvc;
using SharedLibrary;

namespace Backend_Pixel_Crawer.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase

    {
        // Temporary storage. Deplace with database storange for player coordinates when we get there. 
        private static List<PlayerCoordinates> playerCoordinatesList = new List<PlayerCoordinates>();

        [HttpPost]
        public IActionResult PostCoordinates([FromBody] PlayerCoordinates coordinates)
        {
            // Store or update the player's coordinates
            var existingPlayer = playerCoordinatesList.FirstOrDefault(p => p.PlayerId == coordinates.PlayerId);
            if (coordinates.X < 0 || coordinates.Y < 0)
            {
                return BadRequest("Invalid coordinates");
            }
            if (existingPlayer != null)
            {
                existingPlayer.X = coordinates.X;
                existingPlayer.Y = coordinates.Y;
            }
            else
            {
                playerCoordinatesList.Add(coordinates);
            }

            return Ok();
        }

        [HttpGet("{playerId}")]
        public IActionResult GetCoordinates(string playerId)
        {
            // Retrieve the coordinates of the specified player
            var playerCoordinates = playerCoordinatesList.FirstOrDefault(p => p.PlayerId == playerId);
            if (playerCoordinates != null)
            {
                return Ok(playerCoordinates);
            }
            else
            {
                return NotFound();
            }
        }
    }

}
