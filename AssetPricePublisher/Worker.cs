using AssetPricePublisher.InfraServices;
using AssetPricePublisher.Models;
using AssetPricePublisher.ModelServices;
using Core.Models.Entities;

namespace AssetPricePublisher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            short timeToSleepInMinutes = 1;
            DateTimeOffset timeNow = DateTimeOffset.Now;
            
            AssetsServices assetsServices = new AssetsServices(); //CRIAR COM INJECAO DE DEPENDENCIA
        
            //buscar todos ativos na base de dados
            List<Assets> assets = await assetsServices.GetAssetsName();
            
            //colocar preço em cada um deles por id ativo
            List<PricedAsset> pricedAssets = assetsServices.LoadPricedAssetsFromAssets(assets);
            
            if (timeNow.Hour == 12)
            {
                //criar uma mensagem e publicar na mensageria
                var messageBrocker = new KafkaClient("localhost:9092", "producer1", 0);
                
                foreach (var assetPriced in pricedAssets)
                {
                    var jsonMessage = System.Text.Json.JsonSerializer.Serialize(assetPriced);
                    var byteMessage = System.Text.Encoding.UTF8.GetBytes(jsonMessage);
                    
                    await messageBrocker.PublishAsync(byteMessage);
                    
                    _logger.LogInformation("Message sent: {message}", jsonMessage);
                }
            }
            else
            {
                _logger.LogInformation("Worker will run again at 12:00:00");
                await Task.Delay(TimeSpan.FromMinutes(timeToSleepInMinutes), stoppingToken);
            }
        }
    }
}