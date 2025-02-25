﻿using Core.DAL.Abstractions;
using MySqlConnector;

namespace Core.DAL.Mysql;

public class ClientsDal : MysqlAbstraction, IClientDal
{
    public ClientsDal(string server, string userName, string password, string databaseName, int port) 
        : base(server, userName, password, databaseName, port)
    {
        TableName = "Clients";
    }
    
    public async Task PersistClientsAsync(List<Client> clients)
    {
        if (clients == null || clients.Count == 0)
            return;
        
        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var query = $"INSERT INTO {TableName} (Name) VALUES (@Name)";
            await using var command = new MySqlCommand(query, connection, transaction);
            
            command.Parameters.Add("@Name", MySqlDbType.VarChar);
            
            foreach (var client in clients)
            {
                if(string.IsNullOrWhiteSpace(client.Name))
                    continue;
            
                command.Parameters["@Name"].Value = client.Name;
                await command.ExecuteNonQueryAsync();
            }
        
            await transaction.CommitAsync();
        }
        catch(Exception e)
        {
            await transaction.RollbackAsync();
        }
    }

    public async Task<List<Client>> GetClientsAsync(short limit = 100)
    {
        var clientList = new List<Client>();

        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);

        await connection.OpenAsync();

        var query = $"SELECT Id, Name FROM {DatabaseName}.{TableName}";
        await using var command = new MySqlCommand(query, connection);

        await using var reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
            return clientList;

        while (await reader.ReadAsync())
        {
            clientList.Add(new Client(
                reader.GetInt32(0), reader.GetString(1))
            );
        }
        
        return clientList;
    }
}