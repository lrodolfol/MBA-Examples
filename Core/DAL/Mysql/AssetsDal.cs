using Core.DAL.Abstractions;
using Core.Models;
using Core.Models.Entities;
using MySqlConnector;

namespace Core.DAL.Mysql;

public class AssetsDal : MysqlAbstraction,  IAssetsDal
{
    public AssetsDal(string server, string userName, string password, string databaseName, int port) 
        : base(server, userName, password, databaseName, port)
    {
        TableName = "Assets";
    }
    
    public async Task<List<Assets>> GetAssetsAsync()
    {
        var assetsList = new List<Assets>();

        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);

        try
        {
            await connection.OpenAsync();

            var query = $"SELECT Id, Name FROM {DatabaseName}.{TableName} ORDER BY RAND()";
            await using var command = new MySqlCommand(query, connection);

            await using var reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
                return assetsList;

            while (await reader.ReadAsync())
            {
                assetsList.Add(new Assets(
                    reader.GetInt32(0), reader.GetString(1))
                );
            }   
        }catch(Exception e)
        {
            ResultTasks.SetMessageError(e.Message);
        }
        
        return assetsList;
    }
}