using Core.Configurations;
using Core.DAL.Mysql;
using Core.Models.Entities;
using EventsConsumer.Models.Exceptions;

namespace EventsConsumer.Services;

public class PositionsService
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    private readonly ILogger<PositionsService> _logger;

    public PositionsService(ILogger<PositionsService> logger) => (_logger) = (logger);

    public async Task UpInsertPositionAsync(Positions positions)
    {
        var dal = new PositionsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );

        await dal.UpInsertPositionAsync(positions);
        if (! dal.ResultTasks.IsSuccess)
            _logger.LogError("Error on insert position -> {0}", dal.ResultTasks.ErrorMessage);
    }
    
}