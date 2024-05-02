using Backend_Pixel_Crawler.Interface;
using Backend_Pixel_Crawler.Managers;
using Backend_Pixel_Crawler.Services;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Backend_Pixel_Crawler.Network.Transport.TCP
{
    public class TcpServer
    {
        private TcpListener _tcpListener;
        private List<SimplifiedLobby> simplifiedLobbies = new List<SimplifiedLobby>();
        IUserAuthenticationService _userAuthenticationService;
        LobbyManager _lobbiesManager;
        TCPSessionManager _sessionManager;
        IConfiguration _configuration;
        PlayerService _playerService;
        public TcpServer(IUserAuthenticationService userAuthenticationService, LobbyManager lobbyManager, TCPSessionManager sessionManager, IConfiguration configuration, PlayerService playerService)
        {

            _userAuthenticationService = userAuthenticationService;
            _lobbiesManager = lobbyManager;
            _sessionManager = sessionManager;
            _configuration = configuration;
            _playerService = playerService;
            

        }

        public async Task StartServer(CancellationToken stoppingToken)

        {
            TCPServerSettings tcpServerSettings = _configuration.GetSection("TcpServerSettings").Get<TCPServerSettings>();
            int port = tcpServerSettings.Port;
            string hostAddress = tcpServerSettings.HostAddress;

            if (tcpServerSettings == null || string.IsNullOrEmpty(tcpServerSettings.HostAddress))
            {
                throw new InvalidOperationException("TCP server settings are not configured correctly.");
            }
            _tcpListener = new TcpListener(System.Net.IPAddress.Parse(hostAddress), port);
            _tcpListener.Start();

            Console.WriteLine("Server started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                TcpClient client = await _tcpListener.AcceptTcpClientAsync();
                Console.WriteLine("New client connected.");
                await HandleClientAsync(client, stoppingToken);
            }
        }


        private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
        {
            Console.WriteLine("Handling client...");


            try
            {
                using (NetworkStream networkStream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    string token = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var userAuth = await _userAuthenticationService.AuthenticateUsersTokenAsync(token);

                    

                    if (userAuth)
                    {

                        try {
                            while (client.Connected)
                            {
                                string userId = await _userAuthenticationService.GetUserIdFromToken(token);

                                var player = await _playerService.FindPlayerInDbByUserID(userId);

                                if (player == null)
                                {

                                    Console.WriteLine("About to start operation that might fail.");

                                    string noPlayerMessage = "Please Type a player name";
                                    byte[] noPlayerData = Encoding.UTF8.GetBytes(noPlayerMessage);
                                    await networkStream.WriteAsync(noPlayerData, 0, noPlayerData.Length);

                                    bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                                    string playerName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                                    if (!string.IsNullOrEmpty(playerName))
                                    {

                                        player = new Player(playerName, userId);
                                        await _playerService.AddPlayerToDb(player);

                                        Console.WriteLine($"New player created: {playerName}");

                                    }
                                    else
                                    {
                                        string errorMessage = "No valid name received. Please try again.";
                                        byte[] errorData = Encoding.UTF8.GetBytes(errorMessage);
                                        await networkStream.WriteAsync(errorData, 0, errorData.Length);
                                    }
                                }

                                if (player != null)
                                {

                                    Console.WriteLine("Token is valid. Client authenticated.");
                                    var session = new TCPSession(client, player);

                                    string AuthenticatedMessage = "You are authenticated and a session is created";

                                    session.SendAsync(AuthenticatedMessage);

                                    _sessionManager.AddSession(session);

                                    while (_sessionManager.GetSession(session.SessionId) != null)
                                    {
                                        await AuthenticatedSessionCommands(session, stoppingToken);
                                    }
                                }
                            }
                        }catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Invalid token. Disconnecting client.");

                        string invalidTokenMessage = "You passed an invalid token, login again to gain a new token";
                        byte[] data = Encoding.UTF8.GetBytes(invalidTokenMessage);

                        client.GetStream().Write(data, 0, data.Length);
                        client.Close();
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

            }
        }

        private async Task AuthenticatedSessionCommands(TCPSession session, CancellationToken stoppingToken)
        {

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    TCPCommand command = await session.ReceiveCommandAsync(stoppingToken);


                    switch (command.Command)
                    {
                        case "JOIN":
                            _lobbiesManager.JoinLobby(command.LobbyId, session);
                            break;
                        case "CREATE":
                            _lobbiesManager.CreateLobby(command.LobbyName, command.PlayerName);
                            break;
                        case "LIST_LOBBIES":
                            _lobbiesManager.GetAllLobies();
                            break;
                        case "LEAVE":
                            _lobbiesManager.LeaveLobby(command.LobbyId, session);
                            break;



                    }
                }
            }
            catch { }
        }
    }
}




    //               // while ((readTotal = stream.Read(buffer, 0, buffer.Length)) != 0)
    //        {
    //            string request = Encoding.ASCII.GetString(buffer, 0, readTotal);
    //            string[] tokens = request.Split(',');

//            if (tokens[0] == "JOIN")
//            {
//                string playerId = tokens[1];
//                string playerName = tokens[2];

//                Console.WriteLine($"Player {playerName} with ID {playerId} has requested to join.");

//                // Here, decide how to handle lobby assignment. If using a default lobby or creating a new one:
//                //  Lobby defaultLobby = GetOrCreateDefaultLobby(); // Implement this based on our  lobby management logic.
//                Player newPlayer = new Player(playerId, playerName, client);
//                //   defaultLobby.AddPlayer(newPlayer);

//                // Optionally, broadcast a message to other players in the lobby about the new player.
//                //    defaultLobby.BroadcastMessage($"Player {playerName} has joined the lobby.", newPlayer);
//                // Send "SPAWN_PLAYER" signal to all connected clients
//                SendSpawnPlayerSignal(playerId);

//                // Add player to lobby or handle accordingly
//            }
//            else if (tokens[0] == "CREATE" && tokens.Length >= 3)
//            {
//                string playerId = tokens[1];
//                string playerName = tokens[2];
//                string lobbyName = tokens[3];
//                Guid myuuid = Guid.NewGuid();
//                string myuuidAsString = myuuid.ToString();

//                Lobby newLobby = new Lobby(myuuidAsString, lobbyName, playerName) ;

//                Player newPlayer = new Player(playerId, playerName, client);

//                newLobby.AddPlayer(newPlayer);
//                Console.WriteLine("looby id:" + myuuidAsString);

//                Console.WriteLine(newLobby);

//                lobbies.Add(newLobby);


//                Console.WriteLine(lobbies);

//                Console.WriteLine($"New lobby with the name {lobbyName} created by player {playerId} with name {playerName}");


//            }
//            else if (tokens[0] == "LIST_LOBBIES")
//            {


//                foreach (Lobby lobby in lobbies)
//                {
//                    if (lobby.Players.Count > 0) // Ensure there's at least one player
//                    {
//                        var simpleInfo = new SimplifiedLobby
//                        {
//                            LobbyId = lobby.LobbyId,
//                            LobbyName = lobby.LobbyName, // Assuming you've added a LobbyName property
//                            CreatorName = lobby.Players.First().Name // Assuming the first player is the creator
//                        };
//                        simplifiedLobbies.Add(simpleInfo);
//                    }


//                }


//                string json = JsonSerializer.Serialize(simplifiedLobbies);
//                byte[] data = Encoding.UTF8.GetBytes(json);
//                client.GetStream().Write(data, 0, data.Length);

//                Console.WriteLine(lobbies);

//            }

//            else if (tokens[0] == "Space")
//            {
//                string action = tokens[0];
//                string playerId = tokens[1];
//                string content = tokens[2];
//                Console.WriteLine($"action: {action}, playerID: {playerId}, JsonString: {content}");                    
//            }                
//        }

//        //finally{ }

//        stream.Close();
//        client.Close();
//    }

//    private void SendSpawnPlayerSignal(string playerId)
//    {
//        Console.WriteLine("Sending SPAWN_PLAYER signal...");

//        // Construct the message to send to clients
//        string message = $"SPAWN_PLAYER,{playerId}";

//        // Convert the message to bytes
//        byte[] dataToSend = Encoding.UTF8.GetBytes(message);

//        // Send the message to all connected clients
//        foreach (var client in _connectedClients)
//        {
//            client.GetStream().Write(dataToSend, 0, dataToSend.Length);
//        }

//        Console.WriteLine("SPAWN_PLAYER signal sent to all clients.");
//    }

//    private void SendRemovePlayerSignal(string playerId)
//    {
//        Console.WriteLine("Sending REMOVE_PLAYER signal...");

//        // Construct the message to send to clients
//        string message = $"REMOVE_PLAYER,{playerId}";

//        // Convert the message to bytes
//        byte[] dataToSend = Encoding.UTF8.GetBytes(message);

//        // Send the message to all connected clients
//        foreach (var client in _connectedClients)
//        {
//            client.GetStream().Write(dataToSend, 0, dataToSend.Length);
//        }

//        Console.WriteLine("REMOVE_PLAYER signal sent to all clients.");
//    }

//}

