namespace AssetPricePublisher.InfraServices;

public interface IPricedAssetService
{
    public Task<bool> PublishAsync(byte[] message);
}