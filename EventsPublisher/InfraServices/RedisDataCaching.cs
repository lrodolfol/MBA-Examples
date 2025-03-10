using System.Text.Json;
using StackExchange.Redis;

namespace EventsPublisher.InfraServices;

public class RedisDataCaching
{
    private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new(() =>
    {
        var config = new ConfigurationOptions
        {
            EndPoints = { "127.0.0.1:6379" },
            Password = "sinqia123",
            
            AbortOnConnectFail = false
        };
        return ConnectionMultiplexer.Connect(config);
    });
    private readonly IDatabase _database;
    public bool IsConnected => LazyConnection.Value.IsConnected; 

    public RedisDataCaching()
    {
        _database = LazyConnection.Value.GetDatabase();
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration)
    {
        string jsonData = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, jsonData, expiration);
    }
    
    public async Task AddToListAsync(string key, string value)
    {
        await _database.ListRightPushAsync(key, value);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        string jsonData = await _database.StringGetAsync(key);
        return string.IsNullOrEmpty(jsonData) ? default : JsonSerializer.Deserialize<T>(jsonData);
    }
    
    public async Task<List<string>> GetListAsync(string key)
    {
        var values = await _database.ListRangeAsync(key);
        var result = new List<string>();

        foreach (var value in values)
        {
            result.Add(value.ToString());
        }

        return result;
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}