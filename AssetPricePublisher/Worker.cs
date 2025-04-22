using AssetPricePublisher.InfraServices;
using AssetPricePublisher.Models;
using AssetPricePublisher.ModelServices;
using Core.Models.Entities;

namespace AssetPricePublisher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AssetsServices _assetsServices;
    private readonly IConfiguration _configuration;

    public Worker(IServiceScopeFactory scope, ILogger<Worker> logger, IConfiguration configuration)
    {
        _assetsServices = scope.CreateScope().ServiceProvider.GetRequiredService<AssetsServices>();
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var updatedForIntraday = false;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            short timeToSleepInMinutes = 1;
            
            DateTimeOffset timeNow = DateTimeOffset.Now;
            
            if(timeNow.Hour > 6 && !updatedForIntraday)
            {
                try
                {
                    _logger.LogInformation("Starting Asset Price Publisher");
                    
                    await RunTaskForPublish(stoppingToken, timeToSleepInMinutes);
                    updatedForIntraday = true;
                }catch (Exception e)
                {
                    _logger.LogError("Error in worker - {error}", e.Message);
                }
            }else if (timeNow.Hour < 6)
            {
                updatedForIntraday = false;
                _logger.LogInformation("Worker will run again after 06:00 AM today");
            }
            else
            {
                _logger.LogInformation("Worker will run again after 06:00 AM at {day}", 
                    DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd")
                    );
                await Task.Delay(TimeSpan.FromMinutes(timeToSleepInMinutes), stoppingToken);
            }
        }
    }

    private async Task RunTaskForPublish(CancellationToken stoppingToken, short timeToSleepInMinutes)
    {
         List<Assets> assets = await _assetsServices.GetAssets();
         if (assets.Count <= 0)
         {
             await Task.Delay(TimeSpan.FromMinutes(timeToSleepInMinutes), stoppingToken);
             _logger.LogInformation("Assets are empty, will run again in {timeToSleepInMinutes} minutes", timeToSleepInMinutes);
                 
             return;
         }
         
         var pricedAssets = _assetsServices.LoadPricedAssetsFromAssets(assets);
         
         var kafkaProperties = GetKafkaProperties();
         var messageBrocker = new KafkaClient(
             kafkaProperties.bootstrapServer,
             kafkaProperties.topicName,
             kafkaProperties.partitionsNumber,
             kafkaProperties.retentionTtlPerHour
             );
                
        foreach (var assetPriced in pricedAssets)
        {
            var jsonMessage = System.Text.Json.JsonSerializer.Serialize(assetPriced);
            var byteMessage = System.Text.Encoding.UTF8.GetBytes(jsonMessage);
                    
            if(await messageBrocker.PublishAsync(byteMessage))
                _logger.LogInformation("Message published to topic {topic} - {message}", kafkaProperties.topicName, jsonMessage);
            else
                _logger.LogError("Fail to publishMessage, Will be send to cache - {message}", jsonMessage);
        }
    }
    
    private (string bootstrapServer, string topicName, int partitionsNumber, int retentionTtlPerHour) GetKafkaProperties()
    {
        var bootstrapServers 
            = _configuration["MessageBroker:Kafka:BootstrapServers"] ?? "localhost:9092";
        var topic 
            = _configuration["MessageBroker:Kafka:Topic"] ?? "assetsPriced";
        var partition
            = Convert.ToInt16(_configuration["MessageBroker:Kafka:PartitionsNumbers"] ?? "3");
        var replicationFactor 
            = Convert.ToInt16(_configuration["MessageBroker:Kafka:RetentionTtlPerHour"] ?? "1");

        return (bootstrapServers, topic, partition, replicationFactor);
    }
}