using Confluent.Kafka;
namespace AssetPricePublisher.InfraServices;

public class KafkaClient : IPricedAssetService
{
    private readonly string _bootstrapServers;
    private readonly string _topic;
    private readonly int _partition;
    private readonly string _username;
    private readonly string _password;
    
    private readonly ProducerConfig _producerConfig;
    
    public KafkaClient(string bootstrapServers, string topic, int partition)
    {
        _bootstrapServers = bootstrapServers;
        _topic = topic;
        _partition = partition;
        
        _producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
    }

    private bool PropertiesIsInvalid()
    {
        var invalid = false;
        
        if (string.IsNullOrWhiteSpace(_topic))
            invalid = true;

        if (string.IsNullOrWhiteSpace(_bootstrapServers))
            invalid = true;

        if (string.IsNullOrWhiteSpace(_username))
            invalid = true;

        if (string.IsNullOrWhiteSpace(_password))
            invalid = true;

        return invalid;
    }

    public async Task PublishAsync(byte[] message)
    {
        if(PropertiesIsInvalid())
            throw new Exception("Kafka properties are invalid. Please set the properties before publishing.");

        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
        
        var result = await producer.ProduceAsync(_topic, new Message<Null, string>
        {
            Key = null,
            Value = System.Text.Encoding.UTF8.GetString(message)
        });
        
        Console.WriteLine($"Mensagem enviada para {result.TopicPartitionOffset}");
    }
}