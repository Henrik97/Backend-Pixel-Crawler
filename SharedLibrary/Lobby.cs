using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class Lobby
    {
        public string LobbyId { get; set; }

        public string LobbyName { get; set; }    
        public string HostName { get; set; }
        public List<Player> Players { get; set; }
        public bool isGameStarted { get; set; }

        public Lobby(string lobbyId, string lobbyName, string hostName) {

            LobbyId = lobbyId;
            LobbyName = lobbyName;
            HostName = hostName;
            Players = new List<Player>();
            isGameStarted = false;
        
        }

        public void AddPlayer(Player player) {

            BroadcastMessage($"Player {player.Name} has joined the lobby.", player);

            Players.Add(player);

        }

        public void RemovePlayer(Player player) { }

        public void BroadcastMessage(string message, Player excludedPlayer = null) { 
            
            foreach (Player player in Players) {
                player.SendMessage(message);
            
            }
        
        }
    }
}
