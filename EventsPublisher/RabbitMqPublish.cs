using Core.Configurations;
using RabbitMQ.Client;

namespace EventsPublisher;

public class RabbitMqPublish
{
    private readonly string _exchangeName = "publisher-events-exchange";
    private readonly string _queueName = "publisher-events-queue";
    private readonly string _routingKey = "events";
    
    private readonly string _exchangeNameDeadLeattler = $"publisher-events-exchange-dead-leattler";
    private readonly string _queueNameDeadLeattler  = "publisher-events-queue-dead-leattler";
    
    private readonly MyConfigurations.RabbitMqConfiguration _rabbitMqConfiguration = MyConfigurations.RabbitMqEnvironment;
    
    private ConnectionFactory _factory;
    private Dictionary<string,object> _queueArguments;

    public RabbitMqPublish()
    {
        CreateFactory();
        LoadQueueArguments();
    }
    public async Task Publish()
    {
        await using IConnection connection = await _factory.CreateConnectionAsync();
        await using IChannel chanel = await connection.CreateChannelAsync();
        
        await chanel.ExchangeDeclareAsync(_exchangeNameDeadLeattler, ExchangeType.Topic, true, false, null);
        QueueDeclareOk queueDeclareOkDeadLeatter = await chanel.QueueDeclareAsync(_queueNameDeadLeattler, true, false, false, null);
        await chanel.QueueBindAsync(queueDeclareOkDeadLeatter.QueueName, _exchangeName, _routingKey, null);
        
        chanel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic, true, false, null);
        QueueDeclareOk queueDeclareOk = await chanel.QueueDeclareAsync(_queueName, true, false, false, _queueArguments!);
        
        await chanel.QueueBindAsync(queueDeclareOk.QueueName, _exchangeName, _routingKey, null);
    }

    private void LoadQueueArguments()
    {
        _queueArguments = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", $"{_exchangeNameDeadLeattler}" },
            { "x-dead-letter-routing-key", _routingKey },
            { "x-message-ttl", TimeSpan.FromMinutes(10).TotalMilliseconds },
            { "x-queue-mode", "lazy"}
        };
    }
    private void CreateFactory()
    {
        _factory = new ConnectionFactory()
        {
            HostName = _rabbitMqConfiguration.Host,
            UserName = _rabbitMqConfiguration.UserName,
            Password = _rabbitMqConfiguration.Password,
            Port = _rabbitMqConfiguration.Port,
            VirtualHost = _rabbitMqConfiguration.VirtualHost,
        };
    }
}