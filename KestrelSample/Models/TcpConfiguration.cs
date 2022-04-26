using System.Collections.Generic;

namespace KestrelSample.Models;

public class TcpConfiguration
{
    public TcpEndpoints TcpEndpoints { get; set; }
}

public class TcpEndpoints
{
    public Dictionary<string, EndpointConfiguration> Endpoints { get; set; }

    public void Update(Services services)
    {
        foreach (var configuration in services.ServicesConfigurations)
        {
            Endpoints[configuration.ServiceName] = new EndpointConfiguration()
            {
                Url = $"http://localhost:{configuration.Port}"
            };
        }
    }
}

public class EndpointConfiguration
{
    public string Url { get; set; }
}

public class Services
{
    public List<ServicesConfiguration> ServicesConfigurations { get; set; }
}

public class ServicesConfiguration
{
    public string ServiceName { get; set; }
    public int Port { get; set; }
}