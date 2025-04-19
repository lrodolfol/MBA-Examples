using Confluent.Kafka;
namespace AssetPricePublisher.InfraServices;

public class kafkaClient : IPricedAssetService
{
    private readonly string bootstrapServers;
    private readonly string topic;
    private readonly int partition = 0;
    private readonly string username;
    private readonly string password;
    private readonly int port;

    public kafkaClient()
    {
        var configKafka = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
    }

    private bool PropertiesIsInvalid()
    {
        var invalid = false;
        
        if (string.IsNullOrWhiteSpace(topic))
            invalid = true;

        if (string.IsNullOrWhiteSpace(bootstrapServers))
            invalid = true;

        if (string.IsNullOrWhiteSpace(username))
            invalid = true;

        if (string.IsNullOrWhiteSpace(password))
            invalid = true;

        if (port <= 0 || port > 65535)
            invalid = true;

        return invalid;
    }

    public async Task Publish(byte[] message)
    {
        if(PropertiesIsInvalid())
            throw new Exception("Kafka properties are invalid. Please set the properties before publishing.");
        
        
    }
}