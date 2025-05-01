using Core.DAL.Abstractions;
using Core.Models.Agregates;
using Core.Models.Entities;
using Dapper;
using MySqlConnector;

namespace Core.DAL.Mysql;

public class BlockValidationDal : MysqlAbstraction
{
    public BlockValidationDal(string server, string userName, string password, string databaseName, int port) 
        : base(server, userName, password, databaseName, port)
    {
        TableName = "Blocks";
    }
    
    public async Task ValidatePossiblesBlocksAsync(int clientId, int assetId)
    {
        var summaryOperarions = await GetTotalSumaryOperations(clientId, assetId);
        var summaryPositions = GetSummaryPositions(clientId, assetId);
    }

    private async Task<List<OperationsDailySummary>> GetTotalSumaryOperations(int clientId, int assetId)
    {
        var listSummary = new List<OperationsDailySummary>();
        
        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();
        
        var query = @$"SELECT CONCAT(ClientId, '-', AssetId) AS 'clientId-assetId', DateOperation,
                    SUM(CASE WHEN OperationType = 'INPUT' THEN (Amount) ELSE (Amount * -1) END) AS 'TotalOperation',
                    SUM(
    		                SUM(CASE WHEN OperationType = 'INPUT' THEN (o.Amount) ELSE (Amount * -1) END)
    	                )
                    OVER (PARTITION BY CONCAT(ClientId, '-', AssetId) ORDER BY CONCAT(ClientId, '-', AssetId), DateOperation ASC) AS 'Accrued'
                    FROM @OperationTableName o
                    WHERE ClientId = @ClientId AND AssetId = @AssetId
                    GROUP BY CONCAT(ClientId, '-', AssetId), DateOperation
                    ORDER BY CONCAT(ClientId, '-', AssetId), DateOperation ASC";
        
        await using var command = new MySqlCommand(query, connection);
            
        command.Parameters.Add("@ClientId", MySqlDbType.Int16);
        command.Parameters.Add("@AssetId", MySqlDbType.Int16);
        command.Parameters.Add("@OperationTableName", MySqlDbType.String);
        
        command.Parameters["@ClientId"].Value = clientId;
        command.Parameters["@AssetId"].Value = assetId;
        command.Parameters["@OperationTableName"].Value = "Operations";
            
        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            listSummary.Add(new OperationsDailySummary(
                reader["clientId-assetId"].ToString()!,
                DateOnly.FromDateTime(DateTime.Parse(reader["DateOperation"].ToString()!)),
                Convert.ToInt16(reader["TotalOperation"]),
                Convert.ToInt16(reader["Accrued"])
            ));
        }
        
        return listSummary;
    }
    
    private async Task<List<Positions>> GetSummaryPositions(int clientId, int assetId)
    {
        var listSummary = new List<Positions>();
        
        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();
        
        var query = @$"SELECT *
                    FROM @PositionTableName p
                    WHERE ClientId = @ClientId AND AssetId = @AssetId
                    ORDER BY CONCAT(ClientId, '-', AssetId), DateOperation ASC";
        
        await using var command = new MySqlCommand(query, connection);
        
        command.Parameters.Add("@ClientId", MySqlDbType.Int16);
        command.Parameters.Add("@AssetId", MySqlDbType.Int16);
        command.Parameters.Add("@PositionTableName", MySqlDbType.String);
        
        command.Parameters["@ClientId"].Value = clientId;
        command.Parameters["@AssetId"].Value = assetId;
        command.Parameters["@OperationTableName"].Value = "Positions";
            
        var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            listSummary.Add(new Positions(
                Convert.ToInt16(reader["ClientId"]),
                Convert.ToInt16(reader["AssetIt"]),
                Convert.ToInt16(reader["Amount"]),
                DateOnly.FromDateTime(DateTime.Parse(reader["DateOperation"].ToString()!))
            ));
        }
        
        return listSummary;
    }
}