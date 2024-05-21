using Backend_Pixel_Crawler.Interface;
using Backend_Pixel_Crawler.Managers;
using Backend_Pixel_Crawler.Services;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
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
        private readonly List<byte> _lastFewBytes = new List<byte>();
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
                    MemoryStream memoryStream = new MemoryStream();
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    string token = null;

                    // Read from the network stream
                    do
                    {
                        bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                        if (bytesRead == 0)
                        {
                            Console.WriteLine("Client disconnected unexpectedly.");
                            return; // Exit if client disconnects prematurely
                        }
                        memoryStream.Write(buffer, 0, bytesRead);

                        if (EndOfMessageDetected(buffer, bytesRead))
                        {
                            token = Encoding.UTF8.GetString(memoryStream.ToArray()).Trim();
                            Console.WriteLine($"Complete message received: {token}");
                            memoryStream.SetLength(0); // Clear memory stream for potential future use

                            if (!string.IsNullOrEmpty(token))
                            {
                                if (await AuthenticateToken(token, networkStream, client))
                                {
                                    Console.WriteLine("Token validated. Starting session.");
                                    await ManageAuthenticatedClient(client, token, stoppingToken);
                                    return; // Exit the method upon successful session management
                                }
                                else
                                {
                                    Console.WriteLine("Invalid token. Disconnecting client.");
                                    return; // Disconnect if the token is invalid
                                }
                            }
                            else
                            {
                                Console.WriteLine("Received empty token. Disconnecting client.");
                                return;
                            }
                        }
                    } while (!stoppingToken.IsCancellationRequested);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading from network stream: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client connection closed.");
            }
        }

        private async Task<bool> AuthenticateToken(string token, NetworkStream networkStream, TcpClient client)
        {
            var userAuth = await _userAuthenticationService.AuthenticateUsersTokenAsync(token.Trim());
            if (!userAuth)
            {
                string invalidTokenMessage = "Invalid token. Please log in again.";
                byte[] data = Encoding.UTF8.GetBytes(invalidTokenMessage);
                await networkStream.WriteAsync(data, 0, data.Length);
                client.Close();
                return false;
            }
            return true;
        }
        private async Task ManageAuthenticatedClient(TcpClient client, string token, CancellationToken stoppingToken)
        {
            NetworkStream networkStream = client.GetStream();
            string userId = await _userAuthenticationService.GetUserIdFromToken(token.Trim());
            Player player = await _playerService.FindPlayerInDbByUserID(userId);

            try
            {
                if (player == null)
                {
                    string noPlayerMessage = "Please type a player name.";
                    byte[] noPlayerData = Encoding.UTF8.GetBytes(noPlayerMessage);
                    await networkStream.WriteAsync(noPlayerData, 0, noPlayerData.Length);

                    MemoryStream memoryStream = new MemoryStream();
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    do
                    {
                        bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                        if (bytesRead == 0)
                        {
                            Console.WriteLine("Client disconnected unexpectedly while entering name.");
                            return; // Exit if client disconnects prematurely
                        }
                        memoryStream.Write(buffer, 0, bytesRead);
                    } while (!EndOfMessageDetected(buffer, bytesRead) && bytesRead > 0);

                    string playerName = Encoding.UTF8.GetString(memoryStream.ToArray()).Trim();
                    memoryStream.SetLength(0);  // Clear the buffer

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
                        return; // Consider exiting or re-prompting, based on your application logic
                    }
                }

                if (player != null)
                {
                    Console.WriteLine("Client authenticated, got a player and a session is ready.");
                    var session = new TCPSession(client, player);
                    string authenticatedMessage = "A player was found under your user name and a session is created, send game commands!";
                    await session.SendAsync(authenticatedMessage);  // Ensure SendAsync is an awaitable method

                    _sessionManager.AddSession(session);

                    while (_sessionManager.GetSession(session.SessionId) != null && !stoppingToken.IsCancellationRequested)
                    {
                        await AuthenticatedSessionCommands(session, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during session management: {ex.Message}");
                // Consider cleaning up or reinitializing the session
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
                        case "MOVEMENT":
                            
                            break;



                    }
                }
            }
            catch { }
        }

bool EndOfMessageDetected(byte[] buffer, int bytesRead)
        {

            if (bytesRead == 0)
                return false;

            _lastFewBytes.AddRange(buffer.Take(bytesRead));

            // Check if the message ends with a newline
            bool endDetected = _lastFewBytes.Contains((byte)'\n');

            // If end is detected, reset the list
            if (endDetected)
                _lastFewBytes.Clear();
            else
            {
                // Keep only the last byte to check for a split newline in the next read
                if (_lastFewBytes.Count > 1)
                    _lastFewBytes.RemoveRange(0, _lastFewBytes.Count - 1);
            }
            Console.WriteLine(endDetected);
            return endDetected;
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

