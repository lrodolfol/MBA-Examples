﻿using Core;
using Core.Configurations;
using EventsPublisher.Services;
using Microsoft.Extensions.Configuration;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev";
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{environment}.json", false)
    .Build();

void StartConfigurations()
{
    MyConfigurations.LoadPropertiesFromEnvironmentVariables();
    configuration.GetSection("messageBrockers:rabbitMq").Bind(MyConfigurations.RabbitMqEnvironment);
}

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
