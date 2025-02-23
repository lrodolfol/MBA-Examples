using Core.DAL.Abstractions;
using MySqlConnector;

namespace Core.DAL;

public class AssetsDal : IAssetsDal
{
    private string Server { get; set; } = null!;
    private string UserName { get; set; } = null!;
    private string Password { get; set; } = null!;
    private string DatabaseName { get; set; } = null!;
    private int Port { get; set; }
    private const string TableName = "Assets";

    private MySqlConnectionStringBuilder _connectionBuilder = null!;

    public AssetsDal(string server, string userName, string password, string databaseName, int port)
    {
        Server = server;
        UserName = userName;
        Password = password;
        DatabaseName = databaseName;
        Port = port;

        CheckConnectionStringEnvironment();

        CreateConnectionString(password, databaseName);
    }

    private void CheckConnectionStringEnvironment()
    {
        if (
            string.IsNullOrWhiteSpace(Server) || string.IsNullOrWhiteSpace(UserName) ||
            string.IsNullOrWhiteSpace(Password)
            || string.IsNullOrWhiteSpace(DatabaseName) || Port <= 0
        )
        {
            throw new ArgumentException("Environment for database connection is invalid");
        }
    }

    private void CreateConnectionString(string password, string databaseName)
    {
        _connectionBuilder = new MySqlConnectionStringBuilder()
        {
            Server = Server,
            UserID = UserName,
            Password = password,
            Database = databaseName,
            Port = (uint)Port
        };
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