using Microsoft.AspNetCore.SignalR;
namespace Backend_Pixel_Crawler.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task BroadCastMessage(string message)
        {
            await Clients.Others.SendAsync("OnMessageReceived", message);
        }
    }
}
