using SharedLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Backend_Pixel_Crawler.Network.Transport.TCP
{
    public class TcpServer
    {
        private TcpListener _tcpListener;
        private List<TcpClient> _connectedClients;

        public TcpServer()
        {
            _connectedClients = new List<TcpClient>();
            StartServer();
        }

        public void StartServer()

        {
            int port = 13000;
            string hostAddress = "127.0.0.1";
            _tcpListener = new TcpListener(System.Net.IPAddress.Parse(hostAddress), port);
            _tcpListener.Start();

            Console.WriteLine("Server started...");

            while (true)
            {
                TcpClient client = _tcpListener.AcceptTcpClient();                
             //   ThreadPool.QueueUserWorkItem(HandleClient, client);
                _connectedClients.Add(client); // Add client to list of connected clients
                Console.WriteLine("New client connected.");
                Task.Run(() => HandleClient(client));
            }
        }


        private void HandleClient(TcpClient client)
        {
            Console.WriteLine("Handling client...");

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int readTotal;

            while ((readTotal = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string request = Encoding.ASCII.GetString(buffer, 0, readTotal);
                string[] tokens = request.Split(',');

                if (tokens[0] == "JOIN")
                {
                    string playerId = tokens[1];
                    string playerName = tokens[2];

                    Console.WriteLine($"Player {playerName} with ID {playerId} has requested to join.");

                    // Here, decide how to handle lobby assignment. If using a default lobby or creating a new one:
                    //  Lobby defaultLobby = GetOrCreateDefaultLobby(); // Implement this based on our  lobby management logic.
                    Player newPlayer = new Player(playerId, playerName, client);
                    //   defaultLobby.AddPlayer(newPlayer);

                    // Optionally, broadcast a message to other players in the lobby about the new player.
                    //    defaultLobby.BroadcastMessage($"Player {playerName} has joined the lobby.", newPlayer);
                    // Send "SPAWN_PLAYER" signal to all connected clients
                    SendSpawnPlayerSignal(playerId);

                    // Add player to lobby or handle accordingly
                }
                else if (tokens[0] == "CREATE" && tokens.Length >= 3)
                {
                    string playerId = tokens[1];
                    string playerName = tokens[2];
                    //string lobbyId = tokens[3];

                    Console.WriteLine($"New lobby created by player {playerId} with name {playerName}");

                    
                }
                else if (tokens[0] == "LIST_LOBBIES")
                {
                    // List lobbies or handle accordingly
                }

                else if (tokens[0] == "Space")
                {
                    string action = tokens[0];
                    string playerId = tokens[1];
                    string content = tokens[2];
                    Console.WriteLine($"action: {action}, playerID: {playerId}, JsonString: {content}");                    
                }                
            }

            stream.Close();
            client.Close();
        }

        private void SendSpawnPlayerSignal(string playerId)
        {
            Console.WriteLine("Sending SPAWN_PLAYER signal...");
           
            // Construct the message to send to clients
            string message = $"SPAWN_PLAYER,{playerId}";

            // Convert the message to bytes
            byte[] dataToSend = Encoding.UTF8.GetBytes(message);

            // Send the message to all connected clients
            foreach (var client in _connectedClients)
            {
                client.GetStream().Write(dataToSend, 0, dataToSend.Length);
            }

            Console.WriteLine("SPAWN_PLAYER signal sent to all clients.");
        }

    }
}
