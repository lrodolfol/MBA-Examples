namespace EventsConsumer.Services;

public class BlockValidationService
{
    public void ValidationBlock((int clientId, int assetId) clientAndAssetId)
    {
        // var blockDal = new BlockDal(
        //     MyConfigurations.MysqlEnvironment.Host,
        //     MyConfigurations.MysqlEnvironment.UserName,
        //     MyConfigurations.MysqlEnvironment.Password,
        //     MyConfigurations.MysqlEnvironment.Database,
        //     MyConfigurations.MysqlEnvironment.Port
        // );
        //
        // if (clientAndAssetId.clientId <= 0 || clientAndAssetId.assetId <= 0)
        // {
        //     throw new InvalidClientIdException($"The client id is invalid. Message: {blockCreated}");
        // }
    }
}