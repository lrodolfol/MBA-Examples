using Core.Models.Enums;

namespace Core.Models.Agregates;

public struct Operation
{
    public int ClientId { get; private set; }
    public int AssetId { get; private set; }
    public ushort Amount { get; private set; }
    public OperationType OperationType { get; private set; }
    public DateTimeOffset Moment { get; private set; }
    
    public Operation(int clientId, int assetId, ushort amount, OperationType operationType) =>
        (ClientId, AssetId, Amount, OperationType) = (clientId, assetId, amount, operationType);
}