namespace Core.Models.Agregates;

public struct OperationsDailySummary
{
    public string ClientIdAssetId { get; set; } = null!;
    public DateOnly OperationDate { get; set; }
    public decimal TotalDailyOperation { get; set; }
    public decimal TotalAccrued { get; set; }
    public OperationsDailySummary(string clientIdAssetId, DateOnly operationDate, decimal totalDailyOperation, decimal totalAccrued)
    {
        ClientIdAssetId = clientIdAssetId;
        OperationDate = operationDate;
        TotalDailyOperation = totalDailyOperation;
        TotalAccrued = totalAccrued;
    }
}