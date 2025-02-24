using Bogus;
using Core;
using Core.DAL;

namespace EventsPublisher;

public class ClientServices
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    public List<Client> CreateMockClients()
    {
        Console.WriteLine("Creating new client");
        var bogus = new Faker("pt-br");
    
        var cont = new Random().Next(10, 100);
        var clients = new List<Client>();
    
        do
        {
            var clientName = bogus.Person.FullName;
            clients.Add(new Client(clientName));
            cont--;
        }while(cont > 0);
    
        return clients;
    }

    public async Task PersistClientsAsync(List<Client> clients)
    {
        var dal = new ClientsMysqlDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
    
        await dal.PersistClientsAsync(clients);
    }

    public async Task<List<Client>> GetClientsAsync()
    {
        var dal = new ClientsMysqlDal(
            "localhost",
            "root",
            "sinqia123",
            "investment",
            3306
        );
    
        return await dal.GetClientsAsync();
    }
}