using System.Net.Sockets;
using System.Text;

namespace SharedLibrary
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TcpClient TcpClient { get; set; }  

        public Player(string id, string name, TcpClient tcpClient) {

            Id = id;
            Name = name;
            TcpClient = tcpClient;  
        }

        public void SendMessage(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
           
            NetworkStream stream = TcpClient.GetStream();

            stream.Write(buffer, 0, buffer.Length);
        }

    }

    
}