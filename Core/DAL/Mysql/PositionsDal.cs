﻿using Core.DAL.Abstractions;
using Core.Models;
using Core.Models.Entities;
using MySqlConnector;

namespace Core.DAL.Mysql;

public class PositionsDal : MysqlAbstraction
{
    public PositionsDal(string server, string userName, string password, string databaseName, int port) : base(server,
        userName, password, databaseName, port)
    {
        TableName = "Positions";
    }
    
    public async Task UpInsertPositionAsync(Positions positions)
    {
        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        var amountForUpdate = positions.Amount;
        try
        {
            var query = @$"INSERT INTO {TableName} (ClientId, AssetId, Amount, Date) 
                    VALUES (@ClientId, @AssetId, @Amount, @Date)
                    ON DUPLICATE KEY UPDATE amount = COALESCE(amount, 0) + (@AmountForUpdate),
                    UpdatedCount = COALESCE(UpdatedCount, 0) + 1";
            
            await using var command = new MySqlCommand(query, connection, transaction);

            command.Parameters.Add("@ClientId", MySqlDbType.Int16);
            command.Parameters.Add("@AssetId", MySqlDbType.Int16);
            command.Parameters.Add("@Amount", MySqlDbType.Int16);
            command.Parameters.Add("@AmountForUpdate", MySqlDbType.Int16);
            command.Parameters.Add("@Date", MySqlDbType.Date);

            command.Parameters["@ClientId"].Value = positions.ClientId;
            command.Parameters["@AssetId"].Value = positions.AssetId;
            command.Parameters["@Amount"].Value = Math.Abs(positions.Amount);
            command.Parameters["@AmountForUpdate"].Value = amountForUpdate;
            command.Parameters["@Date"].Value = positions.Date;

            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            ResultTasks.SetMessageError(e.Message);
            await transaction.RollbackAsync();
        }
    }
    
}