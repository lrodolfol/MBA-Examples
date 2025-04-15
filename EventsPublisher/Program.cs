using System.Text.Json;
using Core.Configurations;
using Core.DAL.Mysql;
using Core.Models.Enums;
using Core.Models.Events;
using EventsPublisher.InfraServices;
using EventsPublisher.Models;
using EventsPublisher.ModelServices;
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

ServiceProvider serviceProvider = new ServiceCollection()
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
    
    if (operations.Count <= 0)
    {
        logger.LogInformation("No operations found. Waiting before next attempt.");
        await Task.Delay(TimeSpan.FromMinutes(1));
        continue;
    }

    using var cacheService = new RedisDataCaching();
    
    var tradeMessage = new TradeMessage();
    var tasksMessageToCache = new List<Task>();
    using IMessageBrocker messageBrocker = new RabbitMqMessageBrocker<TradeMessage>(tradeMessage);
    
    foreach (OperationCreated operation in operations) //seria legal fazer um chun() aqui
    {
        var messageJsonFormated = JsonSerializer.Serialize(operation);
        if (!await messageBrocker.PreparePublish(messageJsonFormated))
        {
            logger.LogError("The message brocker had a failure to publish the message. The massage will be added to the cache.");
            tasksMessageToCache.Add(cacheService.AddToListAsync(configuration["caching:keysNames:tradesMessageWithError"]!, messageJsonFormated));
            
            continue;
        }
        
        await messageBrocker.PublishAsync();
    }

    if(tasksMessageToCache.Count > 0)
        await Task.WhenAll(tasksMessageToCache);

    short timeToSleepInMinutes = 0;
    logger.LogInformation(
            "{0} Operations have been published. Waiting {1} minute(s) to create new operations.", 
            operations.Count, timeToSleepInMinutes
        );
    await Task.Delay(TimeSpan.FromMinutes(timeToSleepInMinutes));
} while (true);

async Task<List<OperationCreated>> CreateNewClientOperation()
{
    var clientServices = serviceProvider.GetRequiredService<ClientServices>();
    var assetsServices = serviceProvider.GetRequiredService<AssetsServices>();
    
    List<OperationCreated> operations = new List<OperationCreated>();
    
    var rand = new Random();
    // if (rand.Next(1, 10) == 6) // 10% of chance to create new clients. Ramdon rule.
    //     await clientServices.PersistClientsAsync(clientServices.CreateMockClients());

    var clients = await clientServices.GetClientsAsync();
    if (clients.Count <= 0)
    {
        logger.LogWarning("No clients have been created still.");
        return operations;   
    }
    
    var assets = await assetsServices.GetAssetsAsync();
    
    foreach (var client in clients)
    {
        var random = rand.Next(1, assets.Count);
        var amount = rand.Next(1, 1_000);

        operations.Add
        (
            new OperationCreated
            (
                client.Id,
                assets[random].Id,
                (short)amount,
                random % 2 == 0 ?  OperationType.INPUT : OperationType.OUTPUT
            )
        );
    }

    return operations;
}