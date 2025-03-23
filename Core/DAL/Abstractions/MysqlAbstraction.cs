using Core.Models;
using MySqlConnector;

namespace Core.DAL.Abstractions;

public abstract class MysqlAbstraction
{
    public ResultTasks ResultTasks { get; private set; } = new ResultTasks(true);
    protected string Server { get; set; } = null!;
    protected string UserName { get; set; } = null!;
    protected string Password { get; set; } = null!;
    protected string DatabaseName { get; set; } = null!;
    protected int Port { get; set; }
    protected string TableName = null!;
    protected MySqlConnectionStringBuilder _connectionBuilder = null!;
    
    public MysqlAbstraction(string server, string userName, string password, string databaseName, int port)
    {
        Server = server;
        UserName = userName;
        Password = password;
        DatabaseName = databaseName;
        Port = port;

        CheckConnectionStringEnvironment();

        CreateConnectionString(password, databaseName);
    }
    
    protected void CheckConnectionStringEnvironment()
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
    
    protected void CreateConnectionString(string password, string databaseName)
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
}