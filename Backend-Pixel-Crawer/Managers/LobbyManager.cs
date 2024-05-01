using SharedLibrary;

namespace Backend_Pixel_Crawler.Managers
{
    public class LobbyManager
    {
        private Dictionary<string, Lobby> Lobbies = new Dictionary<string, Lobby>();


        public Lobby CreateLobby(string lobbyName, string hostName)
        {
            Lobby lobby = new Lobby(lobbyName, hostName);

            Lobbies.Add(lobby.LobbyId, lobby);

            Console.WriteLine("New Lobby Was created");

            return lobby;
        }

        public bool JoinLobby(string lobbyId, TCPSession session)
        {
            if(Lobbies.ContainsKey(lobbyId))
            {
                Lobby lobby = Lobbies[lobbyId];
                // Add the player to the ConnectedSession list and Players dictionary
                lobby.AddPlayer(session);

                // Notify all other players in the lobby about the new player
                string newPlayerSpawnCommand = $"{{\"command\":\"SPAWN_PLAYER\", \"playerId\":\"{session.Player.PlayerId}\"}}";
                lobby.BroadcastMessage(newPlayerSpawnCommand, session.Player);

                // Notify the new player about all existing players
                foreach (var existingSession in lobby.ConnectedSession)
                {
                    if (existingSession != session)  // Ensure not to include the new player
                    {
                        string existingPlayerSpawnCommand = $"{{\"command\":\"SPAWN_PLAYER\", \"playerId\":\"{existingSession.Player.PlayerId}\"}}";
                        session.SendAsync(existingPlayerSpawnCommand);
                    }
                }
                return true;
            }

            return false;
        }

        public void MovementUpdate(string lobbyId, TCPSession session )
        {
            Lobby lobby = Lobbies[lobbyId];
            // Notify all other players in the lobby about the new move
            string newPlayerSpawnCommand = $"{{\"command\":\"MOVEMENT\", \"playerId\":\"{session.Player.PlayerId}\"}}";
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
    }
}
