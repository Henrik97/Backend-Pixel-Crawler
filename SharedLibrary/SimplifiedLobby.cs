using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class SimplifiedLobby
    {
        public string LobbyId { get; set; }
        public string LobbyName { get; set; }
        public string CreatorName { get; set; } // The name of the lobby creator or first player
    }
}
