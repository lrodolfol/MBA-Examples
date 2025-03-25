using System.Text;
using Core.Configurations;
using Core.Models;
using EventsPublisher.Models;
using RabbitMQ.Client;

namespace EventsPublisher.InfraServices;

public class RabbitMqMessageBrocker<T> : IMessageBrocker, IDisposable where T : EventMessage
{
    private T _modelMessage;
    
    private readonly IConnection _connection;
    private readonly IChannel _chanel;
    
    private readonly MyConfigurations.RabbitMqConfiguration _rabbitMqConfiguration = 
        MyConfigurations.RabbitMqEnvironment;
    
    private ConnectionFactory _factory = null!;
    private Dictionary<string, object>? _queueArguments;

    public ResultTasks ResultTasks = new ResultTasks(true);
    
    public RabbitMqMessageBrocker(T modelMessage)
    {
        _modelMessage = modelMessage;
        
        CreateFactory();
        
        _connection = _factory.CreateConnectionAsync().Result;
        _chanel = _connection.CreateChannelAsync().Result;
        
        LoadQueueArguments();
    }
    public async Task PublishAsync()
    {
        await _chanel.BasicPublishAsync(_modelMessage.Exchange, _modelMessage.RoutingKey,_modelMessage.BodyMessage);
    }

    public async Task<bool> PreparePublish(string messageJsonFormated)
    {
        try
        {
            _modelMessage.BodyMessage = Encoding.UTF8.GetBytes(messageJsonFormated);
            
            await _chanel.ExchangeDeclareAsync(
                _modelMessage.ExchangeDeadLeatter, 
                ExchangeType.Topic, 
                _modelMessage.Durable, 
                _modelMessage.AutoDelete
            );
            
            QueueDeclareOk queueDeclareOkDeadLeatter = await _chanel.QueueDeclareAsync(
                _modelMessage.QueueDeadLeatter, 
                _modelMessage.Durable, 
                _modelMessage.Exclusive, 
                _modelMessage.AutoDelete
            );
            
            await _chanel.QueueBindAsync(
                queueDeclareOkDeadLeatter.QueueName, 
                _modelMessage.ExchangeDeadLeatter, 
                _modelMessage.RoutingKey
            );
        
            await _chanel.ExchangeDeclareAsync(
                _modelMessage.Exchange, 
                ExchangeType.Topic, 
                _modelMessage.Durable, 
                _modelMessage.AutoDelete
            );
            
            QueueDeclareOk queueDeclareOk = await _chanel.QueueDeclareAsync(
                _modelMessage.Queue, 
                _modelMessage.Durable, 
                _modelMessage.Exclusive, 
                _modelMessage.AutoDelete, 
                _queueArguments!
            );
        
            await _chanel.QueueBindAsync(
                queueDeclareOk.QueueName, 
                _modelMessage.Exchange, 
                _modelMessage.RoutingKey
            );   
        }catch(Exception e)
        {
            ResultTasks.SetMessageError(e.Message);
            return false;
        }
        
        return true;
    }

    private void LoadQueueArguments()
    {
        _queueArguments = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", $"{_modelMessage.ExchangeDeadLeatter}" },
            { "x-dead-letter-routing-key", _modelMessage.RoutingKey },
            { "x-message-ttl", 1_000 * 60 * 60 },
            { "x-queue-mode", "lazy"}
        };
    }
    private void CreateFactory()
    {
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
