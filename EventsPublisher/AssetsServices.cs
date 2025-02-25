using Core;
using Core.Configurations;
using Core.DAL.Mysql;

namespace EventsPublisher;

public class AssetsServices
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    public async Task<List<Assets>> GetAssetsAsync()
    {        
        var dal = new AssetsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
        
        return await dal.GetAssetsAsync();
    }
}