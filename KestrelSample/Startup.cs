using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using KestrelSample.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace KestrelSample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var services = JsonSerializer.Deserialize<Services>(await File.ReadAllTextAsync("NewServices.json"));
                var tcpEndpoints = JsonSerializer.Deserialize<TcpConfiguration>(await File.ReadAllTextAsync(Program.ConfigPath));

                await context.Response.WriteAsync(JsonSerializer.Serialize(services, new JsonSerializerOptions() {WriteIndented = true}));
                await context.Response.WriteAsync(JsonSerializer.Serialize(tcpEndpoints, new JsonSerializerOptions() {WriteIndented = true}));

                foreach (var servicesServicesConfiguration in services!.ServicesConfigurations)
                    Program.ConfigurationLoader.Endpoint(servicesServicesConfiguration.ServiceName, Program.ConfigureEndpoint);

                tcpEndpoints!.TcpEndpoints.Update(services);
                await File.WriteAllTextAsync(Program.ConfigPath, JsonSerializer.Serialize(tcpEndpoints, new JsonSerializerOptions() {WriteIndented = true}));
            });
        }
    }
}