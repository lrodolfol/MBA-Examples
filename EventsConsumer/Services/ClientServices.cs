using Core.Configurations;

namespace EventsConsumer.Services;

public class ClientServices
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    private readonly ILogger<ClientServices> _logger;

    public ClientServices(ILogger<ClientServices> logger) => (_logger) = (logger);

    public void GetOperationsByClientId()
    {
        
    }
    
}