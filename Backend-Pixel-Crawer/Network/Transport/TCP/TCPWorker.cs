
namespace Backend_Pixel_Crawler.Network.Transport.TCP
{
    public sealed class TCPWorker(ILogger<TCPWorker> logger) : BackgroundService
    {
      
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = new TcpServer();
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay (1000);    
            }
            throw new NotImplementedException();
        }
    }
   
    }
