using Bogus;
using Core;
using Core.DAL;
using EventsPublisher;
using EventsPublisher.Services;

async Task<List<Operation>> CreateNewClientOperation()
{
    var clientServices = new ClientServices();
    var assetsServices = new AssetsServices();
    
    var rand  = new Random();
    if(rand.Next(0, 2) == 0)
        await clientServices.PersistClientsAsync(clientServices.CreateMockClients());
    
    var clients = await clientServices.GetClientsAsync();
    var assets = await assetsServices.GetAssetsAsync();
    var operations = new List<Operation>();
    
    var randon = 0;
    foreach (var client in clients)
    {
        randon = rand.Next(1, assets.Count);
        var amount = rand.Next(1, 1_000);
        
        operations.Add
        (
            new Operation
            (
                client.Id,
                assets[randon].Id,
                (ushort)amount,
                randon % 2 == 0 ? OperationType.INPUT : OperationType.OUTPUT
            )
        );
    }

    return operations;
}
