﻿using System.Text;
using System.Text.Json;
using Core;
using Core.Configurations;
using Core.DAL.Mysql;
using EventsPublisher;
using EventsPublisher.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev";
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{environment}.json", false)
    .Build();

MyConfigurations.LoadPropertiesFromEnvironmentVariables();
configuration.GetSection("messageBrockers:rabbitMq").Bind(MyConfigurations.RabbitMqEnvironment);

var serviceProvider = new ServiceCollection()
    .AddLogging(config =>
    {
        config.AddConsole();
        config.SetMinimumLevel(LogLevel.Information);
    })
    .AddTransient<ClientsDal>()
    .AddTransient<ClientServices>()
    .AddTransient<AssetsDal>()
    .AddTransient<AssetsServices>()
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

do
{
    var operations = await CreateNewClientOperation();
    IMessageBrocker messageBrocker = new RabbitMqMessageBrocker();

    foreach (var operation in operations)
    {
        var operationsByteFormated = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(operation));
        await messageBrocker.PublishAsync(operationsByteFormated);    
    }

    logger.LogInformation($"{operations.Count} Operations have been published. Waiting 5 minutes to create new operations.");
    Thread.Sleep(60000);
} while (true);

async Task<List<Operation>> CreateNewClientOperation()
{
    var clientServices = serviceProvider.GetRequiredService<ClientServices>();
    var assetsServices = serviceProvider.GetRequiredService<AssetsServices>();
    
    var operations = new List<Operation>();
    
    var rand = new Random();
    if (rand.Next(1, 5) % 2 == 0)
        await clientServices.PersistClientsAsync(clientServices.CreateMockClients());

    var clients = await clientServices.GetClientsAsync();
    if (clients.Count <= 0)
    {
        logger.LogWarning("No clients have been created still.");
        return operations;   
    }
    
    var assets = await assetsServices.GetAssetsAsync();
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