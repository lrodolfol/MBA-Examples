using Core.Configurations;
using Core.DAL.Mysql;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.Models.Events;
using EventsConsumer.Models.Exceptions;

namespace EventsConsumer.Services;

public class OperationsService
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    private readonly ILogger<PositionsService> _logger;

    public OperationsService(ILogger<PositionsService> logger) => (_logger) = (logger);

    public async Task ProcessOperationReceivedAsync(OperationCreated operationCreated)
    {
        var operationsDal = new OperationsDal(
            _mysqlEnvironments.Host,
            _mysqlEnvironments.UserName,
            _mysqlEnvironments.Password,
            _mysqlEnvironments.Database,
            _mysqlEnvironments.Port
        );

        var clientOperations = 
            await operationsDal.GetOperationsByClientAndAssetIdAsync
                (
                    operationCreated.ClientId, 
                    operationCreated.AssetId
                );

        CheckIfClientHasEnoughtBalance(operationCreated, clientOperations);

        await operationsDal.CreateOperationAsync(
            new Operations(
                operationCreated.ClientId, 
                operationCreated.AssetId, 
                operationCreated.Amount, 
                DateOnly.FromDateTime(operationCreated.Moment.DateTime),
                operationCreated.OperationType
            )
        );
    }

    private static void CheckIfClientHasEnoughtBalance(OperationCreated operationCreated, List<Operations> clientOperations)
    {
        if (clientOperations.Count == 0 && operationCreated.OperationType == OperationType.OUTPUT)
            throw new InsufficientBalanceException($"The client does not have enough balance to perform the operation. The client does not have any operations for the asset informed. Message: {operationCreated.Serelize()}");
        
        var totalInput = clientOperations.Where(x => x.OperationType == OperationType.INPUT).Sum(x => x.Amount);
        var totalOutput = clientOperations.Where(x => x.OperationType == OperationType.OUTPUT).Sum(x => x.Amount);
        
        var totalAmount = totalInput - totalOutput;
        totalAmount += operationCreated.OperationType == OperationType.OUTPUT ? 
            (operationCreated.Amount *- 1) : operationCreated.Amount;
        
        if(totalAmount <= 0)
            throw new InsufficientBalanceException($"The client does not have enough balance to perform the operation. The client has a total balance of {totalAmount} and the amount operation is {operationCreated.Amount}. Message {operationCreated.Serelize()}");
    }
}