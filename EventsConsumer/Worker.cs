using Core.Configurations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventsConsumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private ConnectionFactory _factory;
    private IConnection _connection;
    private IChannel _channel;

    private readonly MyConfigurations.RabbitMqConfiguration _rabbitMqConfiguration =
        MyConfigurations.RabbitMqEnvironment;

    public Worker(ILogger<Worker> logger)
    {
        _factory = new ConnectionFactory()
        {
            HostName = _rabbitMqConfiguration.Host ?? "localhost",
            UserName = _rabbitMqConfiguration.UserName ?? "sinqia",
            Password = _rabbitMqConfiguration.Password ?? "sinqia123",
            Port = Convert.ToInt16(_rabbitMqConfiguration.Port ?? "5672"),
            VirtualHost = _rabbitMqConfiguration.VirtualHost ?? "/",
        };
        _connection = _factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var eventingBasicConsumer = new AsyncEventingBasicConsumer(_channel);
            eventingBasicConsumer.ReceivedAsync += this.OnMessage;

            _channel.BasicConsumeAsync("publisher-events-queue", false, eventingBasicConsumer); //mudar auto ack para false

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task OnMessage(object sender, BasicDeliverEventArgs eventArgs)
    {
        var obj = eventArgs.Body.ToArray().ToUTF8String().Deserialize<object>();
        Console.WriteLine($"Receives from routinkey '{eventArgs.RoutingKey}':  {obj}");
        
        await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
    }
}

//colocar dentro do core
public static partial class Extensions
{
    public static string Serialize<T>(this T objectToSerialize) => System.Text.Json.JsonSerializer.Serialize<T>(objectToSerialize);
        
    public static T Deserialize<T>(this string jsonText) => System.Text.Json.JsonSerializer.Deserialize<T>(jsonText);

    public static byte[] ToByteArray(this string text) => System.Text.Encoding.UTF8.GetBytes(text);

    public static string ToUTF8String(this byte[] bytes) => System.Text.Encoding.UTF8.GetString(bytes);

    public static ReadOnlyMemory<byte> ToReadOnlyMemory(this byte[] bytes) => new ReadOnlyMemory<byte>(bytes);

        

}