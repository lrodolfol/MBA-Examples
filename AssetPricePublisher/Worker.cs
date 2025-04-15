using AssetPricePublisher.ModelServices;

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
            var timeNow = DateTimeOffset.Now;
            
            var assetsServices = new AssetsServices(); //CRIAR COM INJECAO DE DEPENDENCIA
        
            var assets = await assetsServices.GetAllAssets();
            var pricedAssets = assetsServices.LoadPricedAssetsFromAssets(assets);
            
            if (timeNow.Hour == 12)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    //buscar todos ativos na base de dados
                    
                    
                    //colocar preço em cada um deles por id ativo
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