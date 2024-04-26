using Microsoft.AspNetCore.Http;
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
        public List<TCPSession> ConnectedSession { get; set; }

        public Dictionary<string, Player> Players { get; set; }
        public bool isGameStarted { get; set; }

        public Lobby( string lobbyName, string hostName) {

            LobbyId = Guid.NewGuid().ToString();
            LobbyName = lobbyName;
            HostName = hostName;
            ConnectedSession = new List<TCPSession>();
            Players = new Dictionary<string, Player>();
            isGameStarted = false;
        
        }

        public void AddPlayer(TCPSession session)
        {

            if (!Players.ContainsKey(session.Player.UserID))
            {
                ConnectedSession.Add(session);
                Players[(session.Player.UserID)] = session.Player;
                session.Player.CurrentLobyId = LobbyId;
            }


            BroadcastMessage($"Player {session.Player.PlayerName} has joined the lobby.", session.Player);

        }

        public void RemovePlayer(TCPSession session) {

            ConnectedSession.Remove(session);
            if (Players.ContainsKey(session.Player.UserID))
            {
                Players.Remove(session.Player.UserID);
                session.Player.CurrentLobyId = null;
            }
        }

        public void BroadcastMessage(string message, Player excludedPlayer = null) { 
            
            foreach (TCPSession session in ConnectedSession) {

                if (session.Player != excludedPlayer)
                    session.SendAsync(message);

                else return;
            }
        
        }
    }
}
