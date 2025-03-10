using Core.Configurations;
using Core.DAL.Mysql;
using Core.Models.Entities;
using Microsoft.Extensions.Logging;

namespace EventsPublisher.ModelServices;

public class AssetsServices
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    private readonly ILogger<AssetsServices> _logger;

    public AssetsServices(ILogger<AssetsServices> logger) => _logger = logger;

    public async Task<List<Assets>> GetAssetsAsync()
    {        
        var dal = new AssetsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
        
        var assets = await dal.GetAssetsAsync();
        if(! dal.ResultTasks.IsSuccess)
            _logger.LogError("Error for get assets -> {0}", dal.ResultTasks.ErrorMessage);
        
        return assets;
    }
}