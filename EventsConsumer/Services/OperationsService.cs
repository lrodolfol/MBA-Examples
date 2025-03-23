using Core.Configurations;
using Core.DAL.Mysql;
using Core.Models.Entities;
using Core.Models.Events;

namespace EventsConsumer.Services;

public class OperationsService
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    
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
        
        var totalAmount = clientOperations.Sum(x => x.Amount);
        
        await operationsDal.CreateOperationAsync(
            new Operations(
                operationCreated.ClientId, 
                operationCreated.AssetId, 
                operationCreated.Amount, 
                DateOnly.FromDateTime(operationCreated.Moment.DateTime)
            )
        );
    }
}