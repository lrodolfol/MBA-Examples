using Bogus;
using Core;
using Core.Configurations;
using Core.DAL.Mysql;
using Microsoft.Extensions.Logging;

namespace EventsPublisher.Services;

public class ClientServices
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    private readonly ILogger<ClientServices> _logger;

    public ClientServices(ILogger<ClientServices> logger) => _logger = logger;

    public List<Client> CreateMockClients()
    {
        var cont = new Random().Next(10, 100);
        var clients = new List<Client>();
    
        do
        {
            var clientName = new Faker("pt_BR").Person.FullName;
            clients.Add(new Client(clientName));
            cont--;
        }while(cont > 0);
    
        return clients;
    }

    public async Task PersistClientsAsync(List<Client> clients)
    {
        var dal = new ClientsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
    
        await dal.PersistClientsAsync(clients);
        _logger.LogInformation($"{clients.Count} clients have been persisted.");
    }

    public async Task<List<Client>> GetClientsAsync()
    {
        var dal = new ClientsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
    
        return await dal.GetClientsAsync();
    }
}