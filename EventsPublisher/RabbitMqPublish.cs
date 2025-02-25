using RabbitMQ.Client;

namespace EventsPublisher;

public class RabbitMqPublish
{
    private readonly string _exchangeName = "publisher-events-exchange";
    private readonly string _queueName = "publisher-events-queue";
    private readonly string _routingKey = "events";
    
    private readonly string _exchangeNameDeadLeattler = $"publisher-events-exchange-dead-leattler";
    private readonly string _queueNameDeadLeattler  = "publisher-events-queue-dead-leattler";
    
    public async Task Publish()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 5672
        };
        var connection = await factory.CreateConnectionAsync();
        var chanel = await connection.CreateChannelAsync();
        
        await chanel.ExchangeDeclareAsync(_exchangeNameDeadLeattler, ExchangeType.Topic, true, false, null);
        QueueDeclareOk queueDeclareOkDeadLeatter = await chanel.QueueDeclareAsync(_queueNameDeadLeattler, true, false, false, null);
        await chanel.QueueBindAsync(queueDeclareOkDeadLeatter.QueueName, _exchangeName, _routingKey, null);
        
        chanel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic, true, false, null);
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", $"{_exchangeNameDeadLeattler}" },
            { "x-dead-letter-routing-key", _routingKey }
        };
        QueueDeclareOk queueDeclareOk = await chanel.QueueDeclareAsync(_queueName, true, false, false, args);
        await chanel.QueueBindAsync(queueDeclareOk.QueueName, _exchangeName, _routingKey, null);
    }
}