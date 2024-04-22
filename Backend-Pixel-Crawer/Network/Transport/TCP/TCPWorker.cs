
namespace Backend_Pixel_Crawler.Network.Transport.TCP
{
    public sealed class TCPWorker(ILogger<TCPWorker> logger) : BackgroundService
    {
 
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = new TcpServer();
            await server.StartServer(stoppingToken);
        }
    }
   
    }
