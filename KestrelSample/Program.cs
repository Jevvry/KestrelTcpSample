using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KestrelSample
{
    public class Program
    {
        public static KestrelConfigurationLoader ConfigurationLoader;

        public static readonly string ConfigPath = "Ports.json";
        private static readonly IConfigurationRoot ConfigurationRoot = new ConfigurationBuilder().AddJsonFile(ConfigPath, false, true).Build();

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MyEchoConnectionHandler>());
                })
                .UseKestrel(options =>
                {
                    // Catch configuration loader
                    var section = ConfigurationRoot.GetSection("TcpEndpoints");
                    ConfigurationLoader = options.Configure(section, true);
                    
                    // HTTP 5000
                    options.ListenLocalhost(5000);
                })
                .UseStartup<Startup>();

        public static void ConfigureEndpoint(EndpointConfiguration configuration)
        {
            configuration.ListenOptions.UseConnectionHandler<MyEchoConnectionHandler>();
        }
        
    }
}