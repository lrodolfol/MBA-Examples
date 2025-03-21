using Bogus;
using Core.Configurations;
using Core.DAL.Mysql;
using Core.Models.Entities;
using Microsoft.Extensions.Logging;

namespace EventsPublisher.ModelServices;

public class ClientServices
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    private readonly ILogger<ClientServices> _logger;

    public ClientServices(ILogger<ClientServices> logger) => _logger = logger;

    public List<Clients> CreateMockClients()
    {
        var cont = new Random().Next(10, 100);
        var clients = new List<Clients>();
    
        do
        {
            var clientName = new Faker("pt_BR").Person.FullName;
            clients.Add(new Clients(clientName));
            cont--;
        }while(cont > 0);
    
        return clients;
    }

    public async Task PersistClientsAsync(List<Clients> clients)
    {
        var dal = new ClientsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
    
        await dal.PersistClientsAsync(clients);
        
        if(! dal.ResultTasks.IsSuccess)
            _logger.LogError("Error for persisti clientes -> {0}", dal.ResultTasks.ErrorMessage);
        else
            _logger.LogInformation("{0} clients have been persisted.", clients.Count);
    }

    public async Task<List<Clients>> GetClientsAsync()
    {
        var dal = new ClientsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
    
        var clients = await dal.GetClientsAsync();
        if(!dal.ResultTasks.IsSuccess)
            _logger.LogError("Error for get clients -> {0}", dal.ResultTasks.ErrorMessage);
        
        return clients;
    }
}