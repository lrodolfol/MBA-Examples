namespace AssetPricePublisher.InfraServices;

public interface IPricedAssetService
{
    public Task Publish(byte[] message);
}