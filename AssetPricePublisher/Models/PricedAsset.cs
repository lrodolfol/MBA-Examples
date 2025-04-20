namespace AssetPricePublisher.Models;

public struct PricedAsset
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    public PricedAsset(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}