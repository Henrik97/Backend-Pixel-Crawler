
using Backend_Pixel_Crawler.Interface;
using Backend_Pixel_Crawler.Managers;
using Backend_Pixel_Crawler.Services;
using Microsoft.Extensions.Configuration;

namespace Backend_Pixel_Crawler.Network.Transport.TCP
{
    public sealed class TCPWorker : BackgroundService
    {
        private readonly ILogger<TCPWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly LobbyManager _lobbyManager;
        private readonly TCPSessionManager _sessionManager;
        private readonly IConfiguration _configuration;
        private readonly PlayerService _playerService;
        private readonly IUserService _userService;
        private TcpServer _server;



        public TCPWorker(ILogger<TCPWorker> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {

            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var userAuthenticationService = scope.ServiceProvider.GetRequiredService<IUserAuthenticationService>();
                    var lobbyManager = scope.ServiceProvider.GetRequiredService<LobbyManager>();
                    var sessionManager = scope.ServiceProvider.GetRequiredService<TCPSessionManager>();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    var playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
                    _server = new TcpServer(
                           userAuthenticationService,
                           lobbyManager,
                           sessionManager,
                           _configuration,
                             playerService,
                           userService);


                    await _server.StartServer(stoppingToken);
                }
            }
        }

    }
}
