using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientEcho
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Connecting to port 5019");

            await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 5027));


            while (true)
            {
                var line = Console.ReadLine();
                var message = Encoding.UTF8.GetBytes(line);
                await clientSocket.SendAsync(new ArraySegment<byte>(message), SocketFlags.None);
            }
        }
    }
}