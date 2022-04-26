using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace KestrelSample;

public class ServiceTcpClient : IDisposable
{
    private readonly IPEndPoint endPoint;
    private readonly Socket socket;

    internal ServiceTcpClient(IPEndPoint endPoint)
    {
        this.endPoint = endPoint;
        socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    }

    public static async Task<ServiceTcpClient> CreateForLocalHost()
    {
        var client = new ServiceTcpClient(new IPEndPoint(IPAddress.Loopback, 5051));
        await client.ConnectAsync();
        return client;
    }

    public static async Task<ServiceTcpClient> Create(IPEndPoint endPoint)
    {
        var client = new ServiceTcpClient(endPoint);
        await client.ConnectAsync();
        return client;
    }

    public async Task SendAsync(ReadOnlyMemory<byte> buffer)
    {
        await socket.SendAsync(buffer, SocketFlags.None);
    }

    public async Task<int> ReceiveAsync(Memory<byte> memory)
    {
        return await socket.ReceiveAsync(memory, SocketFlags.None);
    }

    public void Dispose()
    {
        socket?.Dispose();
    }

    private async Task ConnectAsync() => await socket.ConnectAsync(endPoint);
}