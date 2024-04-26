using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class TCPSession
    {
        public TcpClient Client {  get; set; }

        public NetworkStream Stream { get; set; } 

        public string SessionId { get; set; }

        public DateTime LastStreamActivity { get; set; }

        public Player Player { get; set; } 

        public TCPSession(TcpClient client) { 
        
            Client = client;
            Stream = client.GetStream();
            SessionId = Guid.NewGuid().ToString();
            LastStreamActivity = DateTime.Now;
        }

        public void UpdateLastStreamActivity()
        {
            LastStreamActivity = DateTime.UtcNow;
               

        }

        public async Task SendAsync(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message); 

            if(Stream.CanWrite) {

                await Stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public async Task<TCPCommand> ReceiveCommandAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];
            int bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);
            string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            UpdateLastStreamActivity();

            try
            {
                TCPCommand command = JsonSerializer.Deserialize<TCPCommand>(json);
                return command;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing command: {ex.Message}");
                throw;
            }
        }

        public Task CloseAsync()
        {
            Stream.Close();
            Client.Close();
            return Task.CompletedTask;
        }
    }
}
