using Bogus;
using Core;
using Core.DAL;

namespace EventsPublisher;

public class ClientServices
{
    public static List<Client> CreateMockClients()
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

    public static async Task PersistClientsAsync(List<Client> clients)
    {
        var dal = new ClientsMysqlDal(
            "localhost",
            "root",
            "sinqia123",
            "investment",
            3306
        );
    
        await dal.PersistClientsAsync(clients);
    }

    public static async Task<List<Client>> GetClientsAsync()
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