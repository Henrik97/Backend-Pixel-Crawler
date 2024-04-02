using SharedLibrary;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft;
using System.Text.Json;
using Newtonsoft.Json;

namespace Backend_Pixel_Crawler.Network.Transport.TCP
{
    public class TcpServer
    {

        private TcpListener _tcpListener;
        private Dictionary<string, Lobby> lobbies;

        public TcpServer()
        {
            lobbies = new Dictionary<string, Lobby>();
            StartServer();
        }

        public string GetAvailableLobbies()
        {

           var listOfLobbies = lobbies.Values.Select(lobby => new
           {
               Id = lobby.LobbyId,
               Status = lobby.isGameStarted ? "In Progress" : "Waiting for Players"
           }).ToList();
            return JsonConvert.SerializeObject(listOfLobbies);
        }

       public void StartServer()
        {


            var port = 13000;
            var hostAdress = IPAddress.Parse("127.0.0.1");
            _tcpListener = new TcpListener(hostAdress, port);

            _tcpListener.Start();

            byte[] buffer = new byte[1024];

    
                using TcpClient client = _tcpListener.AcceptTcpClient();
                var tcpStream = client.GetStream();

                int readTotal;
              

                while ((readTotal = tcpStream.Read(buffer, 0, buffer.Length)) != 0)
                {


                string request = Encoding.ASCII.GetString(buffer, 0, readTotal);
                string[] tokens = request.Split(',');

                if (tokens[0] == "JOIN" && tokens.Length >= 4)
                {

                    string lobbyId = tokens[1];
                    string playerId = tokens[2];
                    string playerName = tokens[3];

                    Player player = new Player(playerId, playerName, client);

                    if (lobbies.ContainsKey(lobbyId))
                        {
                        lobbies[lobbyId].AddPlayer(player);

                        var anouncement = Encoding.UTF8.GetBytes($"Player has joined {lobbyId}");

                        tcpStream.Write(anouncement, 0 , anouncement.Length); 

                    }
                    else
                    {

                        var response = Encoding.UTF8.GetBytes("There is no lobby with that ID");
                        tcpStream.Write(response, 0, response.Length);
                    }
                 
                }
                else if (tokens[0] == "CREATE")
                {

                    string playerId = tokens[1];
                    string playerName = tokens[2];

                    Player player = new Player(playerId, playerName, client);
                    int testLobbyID = 1;
                    string lobbyId = testLobbyID.ToString();
                    Lobby newLobby = new Lobby(lobbyId);
                    newLobby.AddPlayer(player);
                    lobbies.Add(lobbyId, newLobby); 

                    var response = Encoding.UTF8.GetBytes(lobbyId);
                    tcpStream.Write(response, 0, response.Length); // Send the lobby ID to the client

                }
                else if (tokens[0] == "LIST_LOBBIES")
                {
                    string playerId = tokens[1];
                    string playerName = tokens[2];
                    Player player = new Player(playerId, playerName, client);
                    string lobbyList = GetAvailableLobbies();
                    player.SendMessage(lobbyList);
                }

            }

            }
        }

    }

