using Core.Configurations;
using Core.DAL.Mysql;
using Core.Helpers;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.Models.Events;
using EventsConsumer.Models.Exceptions;
using EventsConsumer.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventsConsumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private ConnectionFactory _factory;
    private IConnection _connection;
    private IChannel _channel;
    
    IServiceProvider _serviceProvider;

    private readonly MyConfigurations.RabbitMqConfiguration _rabbitMqConfiguration =
        MyConfigurations.RabbitMqEnvironment;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
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
        _channel.BasicQosAsync(0, 1, false);
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var eventingBasicConsumer = new AsyncEventingBasicConsumer(_channel);
            eventingBasicConsumer.ReceivedAsync += this.OnMessage;

            await _channel.BasicConsumeAsync("publisher-events-queue", false, eventingBasicConsumer);

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task OnMessage(object sender, BasicDeliverEventArgs eventArgs)
    {
        // _ = Task.Run(async () =>
        // {
            var message = eventArgs.Body.ToArray().ToUTF8String();
            var operationCreated = message.Deserialize<OperationCreated>();
            
            _logger.LogInformation("Receives from routinkey. Message: {0}", message);

            try
            {
                //colocar UnitOfWork para todos processos.

                var operationsServices = _serviceProvider.GetRequiredService<OperationsService>(); //deixar como propriedade da classe
                await operationsServices.ProcessOperationReceivedAsync(operationCreated);

                var positionsService = _serviceProvider.GetRequiredService<PositionsService>(); //deixar como propriedade da classe
                var amoutConverted = operationCreated.OperationType == OperationType.INPUT
                    ? operationCreated.Amount : operationCreated.Amount * -1;
                
                await positionsService.UpInsertPositionAsync(
                    new Positions(
                        operationCreated.ClientId, 
                        operationCreated.AssetId, 
                        (short)amoutConverted,
                        DateOnly.FromDateTime(operationCreated.Moment.DateTime)
                        )
                    );
                
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
            }
            catch (Exception e) when(e is InvalidBusinessDayException || e is NegativeAmountException)
            {
                _logger.LogError("Error on process operation. Details: {0}", e.Message);
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
            catch (Exception e)
            {
                _logger.LogError("Error on process operation. Details: {0}", e.Message);
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        //});
    }
}
