using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace KestrelSample
{
    public class MyEchoConnectionHandler : ConnectionHandler
    {
        private readonly ILogger<MyEchoConnectionHandler> logger;

        public MyEchoConnectionHandler(ILogger<MyEchoConnectionHandler> logger)
        {
            this.logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            logger.LogInformation(connection.ConnectionId + " connected");
            while (true)
            {
                var result = await connection.Transport.Input.ReadAsync();
                var buffer = result.Buffer;

                foreach (var segment in buffer)
                {
                    logger.LogInformation(Encoding.UTF8.GetString(segment.Span));
                    await connection.Transport.Output.WriteAsync(segment);
                }

                if (result.IsCompleted)
                {
                    break;
                }

                connection.Transport.Input.AdvanceTo(buffer.End);
            }

            logger.LogInformation(connection.ConnectionId + " disconnected");
        }
    }
}