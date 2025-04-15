using AssetPricePublisher.Models;
using Core.DAL.Mysql;
using Core.Models.Entities;

namespace AssetPricePublisher.ModelServices;

public class AssetsServices
{
    private readonly ILogger<AssetsServices> _logger;
    private readonly AssetsDal _assetsDal;
    
    // public AssetsServices(ILogger<AssetsServices> logger, AssetsDal assetsDal)
    // {
    //     _logger = logger;
    //     _assetsDal = assetsDal;
    // }

    public async Task<List<Assets>> GetAllAssets()
    {
        var assets = await _assetsDal.GetAssetsAsync();
        if(! _assetsDal.ResultTasks.IsSuccess)
            _logger.LogError("{nameof} Fail for get assets from database - Error - {error}", nameof(GetAllAssets), _assetsDal.ResultTasks.ErrorMessage);
        
        return assets;
    }

    public List<PricedAsset> LoadPricedAssetsFromAssets(List<Assets> assets)
    {
        var pricedAssets = new List<PricedAsset>();
        var rand = new Random();
        
        assets.ForEach(x =>
        {
            var price = Math.Round((decimal)rand.NextDouble() * 9.99m, 2);
            
            pricedAssets.Add(
                new PricedAsset(x.Id, x.Name, price)
                );    
        });
        
        return pricedAssets;
    }
}