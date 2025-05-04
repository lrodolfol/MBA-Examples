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
    private readonly IPricedAssetService _messageBrockerClient;

    public Worker(IServiceScopeFactory scope, ILogger<Worker> logger, 
        IConfigurationRoot configuration, IPricedAssetService messageBrockerClient)
    {
        _assetsServices = scope.CreateScope().ServiceProvider.GetRequiredService<AssetsServices>();
        _logger = logger;
        _configuration = configuration;
        _messageBrockerClient = messageBrockerClient;
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
                
        foreach (var assetPriced in pricedAssets)
        {
            var jsonMessage = System.Text.Json.JsonSerializer.Serialize(assetPriced);
            var byteMessage = System.Text.Encoding.UTF8.GetBytes(jsonMessage);

            await _messageBrockerClient.PublishAsync(byteMessage);
        }
    }
}