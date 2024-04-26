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
                Lobbies[lobbyId].AddPlayer(session);
                return true;
            }

            return false;
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
