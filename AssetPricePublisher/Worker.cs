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
            List<string> assets = await assetsServices.GetAssetsName();
            
            //colocar preço em cada um deles por id ativo
            List<PricedAsset> pricedAssets = assetsServices.LoadPricedAssetsFromAssets(assets);
            
            if (timeNow.Hour == 12)
            {
                foreach (var assetPriced in pricedAssets)
                {
                    
                }
                
                if (_logger.IsEnabled(LogLevel.Information))
                {
                           
                    
                    //criar uma mensagem e publicar na mensageria
                    
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
            
                await Task.Delay(TimeSpan.FromMinutes(timeToSleepInMinutes), stoppingToken);
            }
            else
            {
                _logger.LogInformation("Worker will run again at 12:00:00");
                await Task.Delay(TimeSpan.FromMinutes(timeToSleepInMinutes), stoppingToken);
            }
        }
    }
}