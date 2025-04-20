namespace AssetPricePublisher.InfraServices;

public interface IPricedAssetService
{
    public Task PublishAsync(byte[] message);
}