using System.Net.Sockets;
using System.Text;

namespace SharedLibrary
{
    public class Player
    {
        public string PlayerId { get; set; }

        public string PlayerName { get; set; }

        public string UserID { get; set; }

        public string CurrentLobbyId { get; set; }

        public Player(string playerId, string playerName, string userID) {

            PlayerId = playerId;
            PlayerName = playerName;
            UserID = userID;

            }

    }

    
}