using Core;
using Core.DAL;

namespace EventsPublisher;

public class AssetsServices
{
    public static async Task<List<Assets>> GetAssetsAsync()
    {
        var dal = new AssetsMysqlDal
        (
            "localhost",
            "root",
            "sinqia123",
            "investment",
            3306
        );

        return await dal.GetAssetsAsync();
    }
}