using Core.DAL.Abstractions;
using Core.Models;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.Models.Events;
using MySqlConnector;

namespace Core.DAL.Mysql;

public class OperationsDal : MysqlAbstraction
{
    public OperationsDal(string server, string userName, string password, string databaseName, int port) 
        : base(server, userName, password, databaseName, port)
    {
        TableName = "Operations";
    }

    public async Task CreateOperationAsync(Operations operation)
    {
        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var query = @$"INSERT INTO {TableName} (ClientId, AssetId, Amount, DateOperation, OperationType) 
                        VALUES (@ClientId, @AssetId, @Amount, @DateOperation, @OperationType)";
            
            await using var command = new MySqlCommand(query, connection, transaction);

            command.Parameters.Add("@ClientId", MySqlDbType.Int16);
            command.Parameters.Add("@AssetId", MySqlDbType.Int16);
            command.Parameters.Add("@Amount", MySqlDbType.Int16);
            command.Parameters.Add("@DateOperation", MySqlDbType.Date);
            command.Parameters.Add("@OperationType", MySqlDbType.String);

            command.Parameters["@ClientId"].Value = operation.ClientId;
            command.Parameters["@AssetId"].Value = operation.AssetId;
            command.Parameters["@Amount"].Value = operation.Amount;
            command.Parameters["@DateOperation"].Value = operation.DateOperation;
            command.Parameters["@OperationType"].Value = operation.OperationType.ToString();

            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            ResultTasks.SetMessageError(e.Message);
            await transaction.RollbackAsync();
            throw new Exception($"Failed to create operation: {e.Message}");
        }
    }
    
    public async Task<List<Operations>> GetOperationsByClientAndAssetIdAsync(int clientId, int assetId)
    {
        var operations = new List<Operations>();
        
        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();

        try
        {
            var query = $"SELECT * FROM {TableName} WHERE ClientId = @ClientId AND AssetId = @AssetId ORDER BY DateOperation";
            await using var command = new MySqlCommand(query, connection);
            
            command.Parameters.Add("@ClientId", MySqlDbType.Int16);
            command.Parameters.Add("@AssetId", MySqlDbType.Int16);
            command.Parameters["@ClientId"].Value = clientId;
            command.Parameters["@AssetId"].Value = assetId;
            
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var operation = new Operations(
                     Convert.ToInt32(reader["clientId"]),
                     Convert.ToInt32(reader["assetId"]),
                     Convert.ToInt16(reader["amount"]),
                    DateOnly.FromDateTime(DateTime.Parse(reader["DateOperation"].ToString()!)),
                     (OperationType)Enum.Parse(typeof(OperationType), reader["operationType"].ToString()!)
                    );
                operations.Add(operation);
            }
            
            return operations;
        }
        catch(Exception e)
        {
            ResultTasks.SetMessageError(e.Message);
            throw new Exception($"Error on get operations by client and asset id. Error -> {e.Message}");
        }
    }
}