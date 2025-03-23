using Core.Configurations;
using Core.DAL.Mysql;
using Core.Models.Entities;

namespace EventsConsumer.Services;

public class PositionsService
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    
    public async Task UpInsertPositionAsync(Positions positions)
    {
        if (positions.Date.DayOfWeek == DayOfWeek.Sunday || positions.Date.DayOfWeek == DayOfWeek.Saturday)
        {
            throw new ("The position date is not a business day for position -> " + positions.ToString());
            return;
        }
        
        if(positions.Amount < 0)
        {
            throw new ("The amount of position is negative for position -> " + positions.ToString());
        }
        
        var dal = new PositionsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
        
        await dal.UpInsertPositionAsync(positions);
    }
    
}