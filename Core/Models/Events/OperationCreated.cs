using Core.Models.Enums;

namespace Core.Models.Events;

public struct OperationCreated
{
    public int ClientId { get; private set; }
    public int AssetId { get; private set; }
    public ushort Amount { get; private set; }
    
    public OperationType OperationType { get; private set; }
    public DateTimeOffset Moment { get; private set; }
    
    public OperationCreated(int clientId, int assetId, ushort amount, OperationType operationType) =>
        (ClientId, AssetId, Amount, OperationType, Moment) = (clientId, assetId, amount, operationType, DateTimeOffset.Now);
}