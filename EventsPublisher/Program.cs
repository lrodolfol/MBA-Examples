using Bogus;
using Core;
using Core.DAL;
using EventsPublisher;

async Task<List<Operation>> CreateNewClientOperation()
{
    var rand  = new Random();
    if(rand.Next(0, 2) == 0)
        await ClientServices.PersistClientsAsync(ClientServices.CreateMockClients());
    
    var clients = await ClientServices.GetClientsAsync();
    var assets = await AssetsServices.GetAssetsAsync();
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
