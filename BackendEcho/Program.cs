using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BackendEcho
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 5051));
            listener.Start();

            while (true)
            {
                Console.WriteLine("Wait connection");
                using var client = await listener.AcceptTcpClientAsync();
                while (true)
                {
                    Console.WriteLine("Connection accepted");
                    var readPipe = PipeReader.Create(client.GetStream());

                    var result = await readPipe.ReadAsync();

                    var buffer = result.Buffer;

                    foreach (var segment in buffer)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(segment.Span));
                    }

                    if (result.IsCompleted)
                        break;
                }
            }
        }
    }
}