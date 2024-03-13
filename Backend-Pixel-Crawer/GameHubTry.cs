using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Backend_Pixel_Crawler
{
    public class GameHubTry : Hub
    {
        // Method to handle receiving coordinates from the client
        public async Task SendCoordinates(int x, int y)
        {
            // Process coordinates as needed
            // For example, broadcast coordinates to all clients
            await Clients.All.SendAsync("ReceiveCoordinates", x, y);
        }
    }
}
