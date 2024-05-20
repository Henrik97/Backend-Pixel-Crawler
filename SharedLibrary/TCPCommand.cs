using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class TCPCommand
    {
        public string Command { get; set; }
        public string? PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string? LobbyId { get; set; }
        public string? Input {  get; set; }
        public string LobbyName { get; set; }

        //Movement related hvis vi vælger at bruge JSON
        public string? Direction { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public string? Move {  get; set; }

    }

    // Kunne ikke få json converter til at genknde enumTyper så endte med at bruge string
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CommandType
    {
        [EnumMember(Value = "JOIN")]
        JoinLobby,
        [EnumMember(Value = "CREATE")]
        CreateLobby,
        [EnumMember(Value = "LEAVE")]
        LeaveLobby,
        [EnumMember(Value = "LIST_LOBBIES")]
        ListLobbies,
        [EnumMember(Value = "SENDMESSAGE")]
        SendMessage,
        [EnumMember(Value = "MOVEMENT")]
        Movement,
        
    }
}
