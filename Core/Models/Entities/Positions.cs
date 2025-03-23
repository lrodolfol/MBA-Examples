namespace Core.Models.Entities;

public class Positions : BaseEntity
{
    public int ClientId { get; private set; }
    public int AssetId { get; private set; }
    public short Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public Positions(int clientId, int assetId, short amount, DateOnly date)
    {
        ClientId = clientId;
        AssetId = assetId;
        Amount = amount;
        Date = date;
    }

    public Positions(int clientId, int assetId, short amount, DateOnly date, DateTime createdAt, DateTime updatedAt)
    {
        ClientId = clientId;
        AssetId = assetId;
        Amount = amount;
        Date = date;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public override string ToString() => $"ClientId: {ClientId}, AssetId: {AssetId}, Amount: {Amount}, Date: {Date}";
}