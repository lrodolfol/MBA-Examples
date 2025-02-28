using Core.Configurations;
using RabbitMQ.Client;

namespace EventsPublisher;

public class RabbitMqMessageBrocker : IMessageBrocker, IDisposable
{
    private readonly string _exchangeName = "publisher-events-exchange";
    private readonly string _queueName = "publisher-events-queue";
    private readonly string _routingKey = "events";
    
    private readonly string _exchangeNameDeadLeattler = $"publisher-events-exchange-dead-leattler";
    private readonly string _queueNameDeadLeattler  = "publisher-events-queue-dead-leattler";
    
    private readonly IConnection _connection;
    private readonly IChannel _chanel;
    
    private readonly MyConfigurations.RabbitMqConfiguration _rabbitMqConfiguration = 
        MyConfigurations.RabbitMqEnvironment;
    
    private ConnectionFactory _factory = null!;
    private Dictionary<string, object>? _queueArguments;

    public RabbitMqMessageBrocker()
    {
        CreateFactory();
        
        _connection = _factory.CreateConnectionAsync().Result;
        _chanel = _connection.CreateChannelAsync().Result;
        
        LoadQueueArguments();
    }
    public async Task PublishAsync(byte[] bodyJsonMessage)
    {
        if (bodyJsonMessage.Length == 0)
            return;
        
        await _chanel.ExchangeDeclareAsync(
            _exchangeNameDeadLeattler, ExchangeType.Topic, true, false
            );
        QueueDeclareOk queueDeclareOkDeadLeatter = await _chanel.QueueDeclareAsync(
            _queueNameDeadLeattler, true, false, false
            );
        await _chanel.QueueBindAsync(queueDeclareOkDeadLeatter.QueueName, _exchangeNameDeadLeattler, _routingKey);
        
        await _chanel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic, true, false);
        QueueDeclareOk queueDeclareOk = await _chanel.QueueDeclareAsync(
            _queueName, true, false, false, _queueArguments!
            );
        
        await _chanel.QueueBindAsync(queueDeclareOk.QueueName, _exchangeName, _routingKey);
        
        await _chanel.BasicPublishAsync(_exchangeName, _routingKey, bodyJsonMessage);
    }

    private void LoadQueueArguments()
    {
        _queueArguments = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", $"{_exchangeNameDeadLeattler}" },
            { "x-dead-letter-routing-key", _routingKey },
            //{ "x-message-ttl", 600000 },
            { "x-queue-mode", "lazy"}
        };
    }
    private void CreateFactory()
    {
        Console.WriteLine(_rabbitMqConfiguration.Host);
        Console.WriteLine(_rabbitMqConfiguration.Port);
        
        _factory = new ConnectionFactory()
        {
            HostName = _rabbitMqConfiguration.Host ?? "localhost",
            UserName = _rabbitMqConfiguration.UserName ?? "sinqia",
            Password = _rabbitMqConfiguration.Password ?? "sinqia123",
            Port = Convert.ToInt16(_rabbitMqConfiguration.Port ?? "5672"),
            VirtualHost = _rabbitMqConfiguration.VirtualHost ?? "/",
        };
    }

    public void Dispose()
    {
        _chanel.CloseAsync();
        _chanel.Dispose();

        _connection.CloseAsync();
        _connection.Dispose();
    }
}
