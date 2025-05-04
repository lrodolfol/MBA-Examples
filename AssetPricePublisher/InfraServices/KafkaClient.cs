using System.Globalization;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace AssetPricePublisher.InfraServices;

public class KafkaClient : IPricedAssetService
{
    private readonly string _bootstrapServers;
    private readonly string _topic;
    private readonly int _partition;
    private readonly int _retentionTtlPerHour;
    
    private readonly ProducerConfig _producerConfig;
    
    private readonly ILogger<KafkaClient> _logger;

    public KafkaClient(string bootstrapServers, string topic, int partition, int retentionTtlPerHour, ILogger<KafkaClient> logger)
    {
        _bootstrapServers = bootstrapServers;
        _topic = topic;
        _partition = partition;
        _retentionTtlPerHour = retentionTtlPerHour;
        
        _producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            MessageTimeoutMs = (int)TimeSpan.FromSeconds(5).TotalMilliseconds,
            Acks = Acks.All,
            EnableIdempotence = false // mensagens na ordem e certeza de entrega (obrigado usar com acks = true)
        };
        
        _logger = logger;
    }

    private async Task CreateTopicIfNotExist()
    {
        try
        {
            using var client = new AdminClientBuilder(_producerConfig).Build();
            var topic = new TopicSpecification()
            {
                Name = _topic,
                NumPartitions = _partition,
                ReplicationFactor = 1,
                Configs = new Dictionary<string, string>
                {
                    { "retention.ms", TimeSpan.FromHours(_retentionTtlPerHour).TotalMilliseconds.ToString(CultureInfo.CurrentCulture) }
                }
            };
            
            await client.CreateTopicsAsync(new List<TopicSpecification> { topic });
        }
        catch (CreateTopicsException e) when (e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
        {
        }
    }

    private bool PropertiesIsInvalid()
    {
        var invalid = false;
        
        if (string.IsNullOrWhiteSpace(_topic))
            invalid = true;

        if (string.IsNullOrWhiteSpace(_bootstrapServers))
            invalid = true;

        return invalid;
    }

    public async Task PublishAsync(byte[] message)
    {
        try
        {
            if(PropertiesIsInvalid())
                throw new Exception("Kafka properties are invalid. Please set the properties before publishing.");
        
            await CreateTopicIfNotExist();

            using var producer = new ProducerBuilder<string, string>(_producerConfig).Build();
        
            var result = await producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString().Substring(0,9),
                Value = System.Text.Encoding.UTF8.GetString(message),
            
            });   
            
            if(result.Status == PersistenceStatus.Persisted)
                _logger.LogInformation(
                    "Message published to topic {topic}#{partition}@{offset} - {message}",
                    result.Topic, result.Partition, result.Offset, result.Value
                    );
            else
                _logger.LogError("Fail to publishMessage, Will be send to cache - {message}", result.Value);
        }catch (Exception e)
        {
            throw;
        }
    }
}