namespace Core.Models.Entities;

public class Blocks(int clientId, int assetId, string description)
{
    public int Id { get; private set; }
    public int ClientId { get; private set; } = clientId;
    public int AssetId { get; private set; } = assetId;
    public string Description { get; private set; } = description;
}