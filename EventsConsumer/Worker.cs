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
    private ushort _consumerCount = 1;
    private readonly ILogger<Worker> _logger;
    private ConnectionFactory _factory;
    private IConnection _connection;
    private IChannel _channel;

    private readonly MyConfigurations.RabbitMqConfiguration _rabbitMqConfiguration =
        MyConfigurations.RabbitMqEnvironment;

    private readonly OperationsService _operationsServices;
    private readonly PositionsService _positionsService;
    private readonly BlockValidationService _blockValidationService;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider,
        BlockValidationService blockValidationService)
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
        _channel.BasicQosAsync(0, _consumerCount, false);
        _logger = logger;

        _positionsService = serviceProvider.GetRequiredService<PositionsService>();
        _operationsServices = serviceProvider.GetRequiredService<OperationsService>();
        _blockValidationService = blockValidationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var eventingBasicConsumer = new AsyncEventingBasicConsumer(_channel);
        eventingBasicConsumer.ReceivedAsync += this.OnMessage;

        await _channel.BasicConsumeAsync("publisher-events-queue", false, eventingBasicConsumer,
            CancellationToken.None);

        //await Task.Delay(1000, stoppingToken);
    }

    private async Task OnMessage(object sender, BasicDeliverEventArgs eventArgs)
    {
        await ProcessMessageAsync(eventArgs);
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs eventArgs)
    {
        var processSuccess = false;
        var message = eventArgs.Body.ToArray().ToUTF8String();
        var operationCreated = message.Deserialize<OperationCreated>();

        //_logger.LogInformation("Receives from routinkey. Message: {0}", message);

        try
        {
            //colocar UnitOfWork para todos processos de escrita na base.

            await _operationsServices.ProcessOperationReceivedAsync(operationCreated);

            var amoutConverted = operationCreated.OperationType == OperationType.INPUT
                ? operationCreated.Amount
                : operationCreated.Amount * -1;

            await _positionsService.UpInsertPositionAsync(
                new Positions(
                    operationCreated.ClientId,
                    operationCreated.AssetId,
                    (short)amoutConverted,
                    DateOnly.FromDateTime(operationCreated.Moment.DateTime)
                )
            );

            await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
            processSuccess = true;
        }
        catch (Exception e) when (e is InvalidBusinessDayException
                                  || e is NegativeAmountException
                                  || e is InsufficientBalanceException)
        {
            _logger.LogError("Error on process operation. Details: {0}", e.Message);
            await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception e)
        {
            _logger.LogError("Error on process operation. Details: {0}", e.Message);
            await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true);
        }

        if(processSuccess)
            await _blockValidationService.BlockWorkFlowAsync((operationCreated.ClientId, operationCreated.AssetId));
    }
}