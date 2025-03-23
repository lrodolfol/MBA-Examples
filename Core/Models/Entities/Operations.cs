namespace Core.Models.Entities;

public class Operations : BaseEntity
{
    public int ClientId { get; private set; }
    public int AssetId { get; private set; }
    public short Amount { get; private set; }
    public DateOnly DateOperation { get; private set; }

    public Operations(int clientId, int assetId, short amount, DateOnly dateOperation)
    {
        ClientId = clientId;
        AssetId = assetId;
        Amount = amount;
        DateOperation = dateOperation;
    }
}