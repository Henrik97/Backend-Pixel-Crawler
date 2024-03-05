using Microsoft.AspNetCore.SignalR;
namespace Backend_Pixel_Crawler
{
    public class ClientHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
        }
    }
}
