using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
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

        public TCPSession(TcpClient client, Player player) { 
        
            Client = client;
            Stream = client.GetStream();
            SessionId = Guid.NewGuid().ToString();
            Player = player;
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
            var completeMessage = new StringBuilder();
            byte[] buffer = new byte[4096];
            int bytesRead;

            string delimiter = "\n";

            try
            {
                do
                {
                    bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    string segment = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    completeMessage.Append(segment);

                    // If the delimiter is found, we've reached the end of the command.
                    if (segment.IndexOf('}') != -1)
                    {
                        // Possible end of JSON object detected
                        // Verify it is not within a string by checking the number of quotes
                        int quotesBeforeBracket = segment.Substring(0, segment.IndexOf('}')).Count(c => c == '"');
                        if (quotesBeforeBracket % 2 == 0)
                        {
                            // Even number of quotes means the bracket is not within a string
                            break;
                        }
                    }
                }
                while (bytesRead > 0); // Continue reading until the stream ends.

                // Trim the complete message to remove the delimiter at the end.
                string fullJsonCommand = completeMessage.ToString().TrimEnd('\n', '\r', ' ');


                Console.WriteLine($"JSON received: {fullJsonCommand}");

                // Update the last stream activity after receiving a full command.
                UpdateLastStreamActivity();

                // Deserialize the full JSON command into a TCPCommand object.
                TCPCommand command = JsonSerializer.Deserialize<TCPCommand>(fullJsonCommand);

                // Log the command for debugging purposes.
                Console.WriteLine(command);

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


