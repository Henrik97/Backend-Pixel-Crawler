using System.Net.Sockets;
using System.Text;

namespace SharedLibrary
{
    public class Player
    {
        public string PlayerId { get; set; } = Guid.NewGuid().ToString();

        public string PlayerName { get; set; }

        public string UserID { get; set; }

        public string? CurrentLobbyId { get; set; }

        public Player() { }
        public Player( string playerName, string userID) {

            PlayerName = playerName;
            UserID = userID;

            }

    }

    
}