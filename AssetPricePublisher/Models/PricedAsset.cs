namespace AssetPricePublisher.Models;

public struct PricedAsset
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    public PricedAsset(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}