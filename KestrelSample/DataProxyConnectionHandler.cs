using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace KestrelSample;

public class DataProxyConnectionHandler : ConnectionHandler
{
    private readonly ILogger<DataProxyConnectionHandler> logger;

    public DataProxyConnectionHandler(ILogger<DataProxyConnectionHandler> logger)
    {
        this.logger = logger;
    }

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        logger.LogInformation("Connected : {ConnectionId}", connection.ConnectionId);

        using var client = await ServiceTcpClient.CreateForLocalHost();

        var timer = Stopwatch.StartNew();
        logger.LogInformation("{ConnectionId} start handling", connection.ConnectionId);

        var read = ReadPipeAsync(client, connection.Transport.Input);
        var write = FillPipeAsync(client, connection.Transport.Output);

        await Task.WhenAll(read, write);

        logger.LogInformation("{ConnectionId} finish handling elapsed {ElapsedMilliseconds}", connection.ConnectionId, timer.ElapsedMilliseconds);
        logger.LogInformation("Disconnected : {ConnectionId}", connection.ConnectionId);
    }

    private static async Task FillPipeAsync(ServiceTcpClient client, PipeWriter writer)
    {
        const int minimumBufferSize = 512;

        while (true)
        {
            try
            {
                // Request a minimum of 512 bytes from the PipeWriter
                var memory = writer.GetMemory(minimumBufferSize);

                var bytesRead = await client.ReceiveAsync(memory);
                if (bytesRead == 0)
                {
                    break;
                }

                // Tell the PipeWriter how much was read
                writer.Advance(bytesRead);
            }
            catch
            {
                break;
            }

            // Make the data available to the PipeReader
            var result = await writer.FlushAsync();

            if (result.IsCompleted)
            {
                break;
            }
        }

        // Signal to the reader that we're done writing
        await writer.CompleteAsync();
    }

    private static async Task ReadPipeAsync(ServiceTcpClient client, PipeReader reader)
    {
        while (true)
        {
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;

            if (buffer.IsSingleSegment)
                await client.SendAsync(buffer.First);
            else
            {
                foreach (var segment in buffer)
                {
                    await client.SendAsync(segment);
                }
            }
            
            // Stop reading if there's no more data coming.
            if (result.IsCompleted)
            {
                break;
            }

            // Tell the PipeReader how much of the buffer has been consumed.
            reader.AdvanceTo(buffer.End);
        }

        // Mark the PipeReader as complete.
        await reader.CompleteAsync();

    }
}