using SharedLibrary;
using StackExchange.Redis;

namespace Backend_Pixel_Crawler.Managers
{
    public class LobbyManager
    {
        private Dictionary<string, Lobby> Lobbies = new Dictionary<string, Lobby>();


        public Lobby CreateLobby(string lobbyName, string hostName)
        {
            Lobby lobby = new Lobby(lobbyName, hostName);

            Lobbies.Add(lobby.LobbyName, lobby);

            Console.WriteLine("New Lobby Was created");

            return lobby;
        }

        public bool JoinLobby(string lobbyName, TCPSession session)
        {
            Console.WriteLine("FIRST JOIN LOBBY PROCESS");
            if(Lobbies.ContainsKey(lobbyName))
            {
                Console.WriteLine("SECOND JOIN LOBBY PROCESS");
                Lobby lobby = Lobbies[lobbyName];
                // Add the player to the ConnectedSession list and Players dictionary
                lobby.AddPlayer(session);

                // Notify all other players in the lobby about the new player
                string newPlayerSpawnCommand = $"{{\"command\":\"SPAWN_PLAYER\", \"playerId\":\"{session.Player.PlayerId}\"}}";
                string sendCommand = $"SPAWN_PLAYER, {session.Player.PlayerId}";
                lobby.BroadcastMessage(sendCommand, session.Player);
                Console.WriteLine("AFTER BroadcastMessage statement in join lobby");
                // Notify the new player about all existing players
                foreach (var existingSession in lobby.ConnectedSession)
                {
                    if (existingSession != session)  // Ensure not to include the new player
                    {
                        string existingPlayerSpawnCommand = $"{{\"command\":\"SPAWN_PLAYER\", \"playerId\":\"{existingSession.Player.PlayerId}\"}}";
                        string existingSendCommand = $"SPAWN_PLAYER, {existingSession.Player.PlayerId}";

                        session.SendAsync(existingSendCommand);
                    }
                }
                return true;
            }

            return false;
        }

        public void MovementUpdate(string lobbyId, string input, TCPSession session )
        {
            Lobby lobby = Lobbies[lobbyId];
            // Notify all other players in the lobby about the new move
            string newPlayerSpawnCommand = $"{{\"command\":\"MOVEMENT\", \"input\":{input}, \"playerId\":\"{session.Player.PlayerId}\"}}";
            lobby.BroadcastMessage(newPlayerSpawnCommand, session.Player);
        }

        public bool LeaveLobby(string lobbyId, TCPSession session) {

            if (Lobbies.ContainsKey(lobbyId))
            {
                Lobbies[(lobbyId)].RemovePlayer(session);
                
                return true;
            }

            return false;
        }

        public Lobby GetLobby(string lobbyId)
        {
            Lobbies.TryGetValue(lobbyId, out Lobby lobby);


            return lobby;

        }

        public List<Lobby> GetAllLobies()
        {
            return Lobbies.Values.ToList();

        }

        public void SendMove(string move, string playerid, string lobbyId)
        {           
            string message = $"MOVEMENT, {move}, {playerid}";
            Lobbies[lobbyId].BroadcastMessage(message, null);
            
        }
    }
}
