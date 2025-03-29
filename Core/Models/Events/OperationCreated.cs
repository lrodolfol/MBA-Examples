using Core.Models.Enums;

namespace Core.Models.Events;

public struct OperationCreated
{
    public int ClientId { get; set; }
    public int AssetId { get; set; }
    public short Amount { get; set; }
    public OperationType OperationType { get; set; }
    public DateTimeOffset Moment { get; set; }
    
    public OperationCreated(int clientId, int assetId, short amount, OperationType operationType) =>
        (ClientId, AssetId, Amount, OperationType, Moment) = (clientId, assetId, amount, operationType, DateTimeOffset.Now);
    
    public override string ToString() 
        => $"ClientId: {ClientId}, AssetId: {AssetId}, Amount: {Amount}, OperationType: {OperationType}, Moment: {Moment}";
}