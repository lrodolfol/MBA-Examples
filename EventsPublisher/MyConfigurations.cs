namespace EventsPublisher;

public static class MyConfigurations
{
    public static MysqlConfiguration MysqlEnvironment { get; set; } = new();

    public static void LoadConfigurations() => LoadMysqlConfiguration();
    
    private static void LoadMysqlConfiguration()
    {
        var dataBaseHost = Environment.GetEnvironmentVariable("MYSQL-DATABASE-HOST") ?? "localhost";
        var dataBaseuserName = Environment.GetEnvironmentVariable("MYSQL-DATABASE-USERNAME") ?? "root";
        var dataBaseserPassword = Environment.GetEnvironmentVariable("MYSQL-DATABASE-PASSWORD") ?? "sinqia123";
        var dataBaseName = Environment.GetEnvironmentVariable("MYSQL-DATABASE-NAME") ?? "investiment_simulator";
        var dataBasePort = Environment.GetEnvironmentVariable("MYSQL-DATABASE-PORT") ?? "3306";
        
        MysqlEnvironment.Host = dataBaseHost;
        MysqlEnvironment.UserName = dataBaseuserName;
        MysqlEnvironment.Password = dataBaseserPassword;
        MysqlEnvironment.Database = dataBaseName;
        MysqlEnvironment.Port = Convert.ToInt16(dataBasePort);
    }
    
    public class MysqlConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}