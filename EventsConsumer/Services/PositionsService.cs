using Core.Configurations;
using Core.Models.Entities;

namespace EventsConsumer.Services;

public class PositionsService
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;

    public async Task UpInsertPositionAsync(Positions positions)
    {
        var query = @"INSERT INTO positions (client_id, asset_id, amount, date) 
                    VALUES (@ClientId, @AssetId, @Amount, @Date, @CreatedAt, @UpdatedAt)
                    ON DUPLICATE KEY UPDATE amount = @Amount, updated_at = @UpdatedAt";
        
        
    }
    
}