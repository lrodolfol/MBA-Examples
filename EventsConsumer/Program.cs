using Core.Configurations;
using Core.DAL.Mysql;
using EventsConsumer;
using EventsConsumer.Services;

var builder = Host.CreateApplicationBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev";
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{environment}.json", false)
    .Build();

MyConfigurations.LoadPropertiesFromEnvironmentVariables();
configuration.GetSection("messageBrockers:rabbitMq").Bind(MyConfigurations.RabbitMqEnvironment);

builder.Services.AddTransient<ClientServices>()
    .AddTransient<OperationsService>()
    .AddTransient<BlockValidationService>()
    .AddTransient<PositionsService>()
    .BuildServiceProvider();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();