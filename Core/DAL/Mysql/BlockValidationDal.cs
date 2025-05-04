using Core.DAL.Abstractions;
using Core.Models;
using Core.Models.Agregates;
using Core.Models.Entities;
using Dapper;
using MySqlConnector;

namespace Core.DAL.Mysql;

public class BlockValidationDal : MysqlAbstraction
{
    public ResultTasks ResultTasks { get; set; } = new();
    
    public BlockValidationDal(string server, string userName, string password, string databaseName, int port) 
        : base(server, userName, password, databaseName, port)
    {
        TableName = "Blocks";
    }

    public async Task<List<OperationsDailySummary>> GetTotalSumaryOperationsAsync(int clientId, int assetId)
    {
        var listSummary = new List<OperationsDailySummary>();
        
        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();
        
        var query = "SELECT CONCAT(ClientId, '-', AssetId) AS 'clientId-assetId', DateOperation, " +
                    " SUM(CASE WHEN OperationType = 'INPUT' THEN (Amount) ELSE (Amount * -1) END) AS 'TotalOperation', " +
                    "SUM( SUM(CASE WHEN OperationType = 'INPUT' THEN (o.Amount) ELSE (Amount * -1) END) ) " +
                    " OVER (PARTITION BY CONCAT(ClientId, '-', AssetId) ORDER BY CONCAT(ClientId, '-', AssetId), DateOperation ASC) AS 'Accrued' " +
                    " FROM Operations o " +
                    " WHERE ClientId = @ClientId AND AssetId = @AssetId " +
                    " GROUP BY CONCAT(ClientId, '-', AssetId), DateOperation " +
                    " ORDER BY CONCAT(ClientId, '-', AssetId), DateOperation ASC";
        
        await using var command = new MySqlCommand(query, connection);
            
        command.Parameters.Add("@ClientId", MySqlDbType.Int16);
        command.Parameters.Add("@AssetId", MySqlDbType.Int16);
        
        command.Parameters["@ClientId"].Value = clientId;
        command.Parameters["@AssetId"].Value = assetId;
            
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
    
    public async Task<List<Positions>> GetSummaryPositionsAsync(int clientId, int assetId)
    {
        var listSummary = new List<Positions>();
        
        try
        {
            await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
            await connection.OpenAsync();
        
            var query = $"SELECT * FROM Positions p WHERE ClientId = @ClientId AND AssetId = @AssetId " +
                        " ORDER BY CONCAT(ClientId, '-', AssetId), CreatedAt ASC";
        
            await using var command = new MySqlCommand(query, connection);
        
            command.Parameters.Add("@ClientId", MySqlDbType.Int16);
            command.Parameters.Add("@AssetId", MySqlDbType.Int16);
        
            command.Parameters["@ClientId"].Value = clientId;
            command.Parameters["@AssetId"].Value = assetId;
            
            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                listSummary.Add(new Positions(
                    Convert.ToInt16(reader["ClientId"]),
                    Convert.ToInt16(reader["AssetId"]),
                    Convert.ToInt16(reader["Amount"]),
                    DateOnly.FromDateTime(DateTime.Parse(reader["CreatedAt"].ToString()!))
                ));
            }
        }
        catch(Exception e)
        {
            ResultTasks.SetMessageError(e.Message);
        }
        
        return listSummary;
    }

    public async Task InsertBlockAsync(Blocks block)
    {
        var query = $"INSERT INTO {TableName} (ClientId, AssetId, Description) VALUES (@ClientId, @AssetId, @Description)";
        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.Add("@ClientId", MySqlDbType.Int16);
        command.Parameters.Add("@AssetId", MySqlDbType.Int16);
        command.Parameters.Add("@Description", MySqlDbType.String);
        
        command.Parameters["@ClientId"].Value = block.ClientId;
        command.Parameters["@AssetId"].Value = block.AssetId;
        command.Parameters["@Description"].Value = block.Description;
        
        var result = await command.ExecuteNonQueryAsync();
    }
    
    public async Task InsertBlockAsync(List<Blocks> blocks)
    {
        if (blocks.Count <= 0)
            return;

        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var query = $"INSERT INTO {TableName} (ClientId, AssetId, Description) VALUES (@ClientId, @AssetId, @Description)";
            await using var command = new MySqlCommand(query, connection, transaction);
            
            command.Parameters.Add("@ClientId", MySqlDbType.Int16);
            command.Parameters.Add("@AssetId", MySqlDbType.Int16);
            command.Parameters.Add("@Description", MySqlDbType.String);
            
            foreach (var block in blocks)
            {
                command.Parameters["@ClientId"].Value = block.ClientId;
                command.Parameters["@AssetId"].Value = block.AssetId;
                command.Parameters["@Description"].Value = block.Description;
                
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch(Exception e)
        {
            ResultTasks.SetMessageError(e.Message);
            await transaction.RollbackAsync();
        }
    }
}