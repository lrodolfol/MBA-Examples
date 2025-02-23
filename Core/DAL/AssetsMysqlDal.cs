using Core.DAL.Abstractions;
using MySqlConnector;

namespace Core.DAL;

public class AssetsMysqlDal : MysqlAbstraction,  IAssetsDal
{
    public AssetsMysqlDal(string server, string userName, string password, string databaseName, int port) 
        : base(server, userName, password, databaseName, port)
    {
        TableName = "Assets";
    }
    
    public async Task<List<Assets>> GetAssetsAsync()
    {
        var assetsList = new List<Assets>();

        await using var connection = new MySqlConnection(_connectionBuilder.ConnectionString);

        await connection.OpenAsync();

        var query = $"SELECT Id, Name FROM {DatabaseName}.{TableName}";
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
        
        return assetsList;
    }
}