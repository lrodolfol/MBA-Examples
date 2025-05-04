using AssetPricePublisher;
using AssetPricePublisher.InfraServices;
using AssetPricePublisher.ModelServices;
using Core.Configurations;
using Core.DAL.Abstractions;
using Core.DAL.Mysql;

var builder = Host.CreateApplicationBuilder(args);

MyConfigurations.LoadPropertiesFromEnvironmentVariables();
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev";

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{environment}.json", false)
    .Build();

builder.Services.AddScoped<AssetsDal>(x => new AssetsDal(
    MyConfigurations.MysqlEnvironment.Host,
    MyConfigurations.MysqlEnvironment.UserName,
    MyConfigurations.MysqlEnvironment.Password,
    MyConfigurations.MysqlEnvironment.Database,
    MyConfigurations.MysqlEnvironment.Port
));
builder.Services.AddScoped<AssetsServices>(AssetsServices => new AssetsServices(
          builder.Services.BuildServiceProvider().GetRequiredService<ILogger<AssetsServices>>(), 
        builder.Services.BuildServiceProvider().GetRequiredService<AssetsDal>()
    )
);
builder.Services.AddSingleton<IConfigurationRoot>(configuration);

var kafkaProperties = GetKafkaProperties(configuration);

builder.Services.AddSingleton<IPricedAssetService>(X =>
{
    var loggerKafka = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<KafkaClient>>();
    
    return new KafkaClient(kafkaProperties.bootstrapServer,
        kafkaProperties.topicName,
        kafkaProperties.partitionsNumber,
        kafkaProperties.retentionTtlPerHour,
        loggerKafka);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();



static (string bootstrapServer, string topicName, int partitionsNumber, int retentionTtlPerHour) 
    GetKafkaProperties(IConfigurationRoot configuration)
{
    var bootstrapServers 
        = configuration["MessageBrocker:Kafka:BootstrapServers"] ?? "localhost:9092";
    var topic 
        = configuration["MessageBrocker:Kafka:Topic"] ?? "asset-price";
    var partition
        = Convert.ToInt16(configuration["MessageBrocker:Kafka:PartitionsNumbers"] ?? "3");
    var replicationFactor 
        = Convert.ToInt16(configuration["MessageBrocker:Kafka:RetentionTtlPerHour"] ?? "1");

    return (bootstrapServers, topic, partition, replicationFactor);
}