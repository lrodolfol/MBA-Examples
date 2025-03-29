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
        if (positions.Date.DayOfWeek == DayOfWeek.Sunday || positions.Date.DayOfWeek == DayOfWeek.Saturday)
        {
            throw new InvalidBusinessDayException("The position date is not a business day for position -> " + positions.ToString());
        }
        
        if(positions.Amount < 0)
        {
            throw new NegativeAmountException("The amount of position is negative for position -> " + positions.ToString());
        }
        
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