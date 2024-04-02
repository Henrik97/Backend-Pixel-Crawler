using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPServer
{
    private UdpClient udpListener;
    private IPEndPoint endPoint;

    public UDPServer(int port)
    {
        // Create UDP listener
        udpListener = new UdpClient(port);
        // Set the endpoint to any IP and the specified port
        endPoint = new IPEndPoint(IPAddress.Any, port);
    }

    public void StartListening()
    {
        try
        {
            Console.WriteLine("UDP server is listening...");

            while (true)
            {
                // Receive incoming data
                byte[] receivedBytes = udpListener.Receive(ref endPoint);
                string receivedData = Encoding.UTF8.GetString(receivedBytes);

                // Display received data
                Console.WriteLine("Received data: " + receivedData);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error occurred: " + e.Message);
        }
    }

    public void Close()
    {
        udpListener.Close();
    }
}