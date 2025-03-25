using Core.DAL.Mysql;
using EventsConsumer;
using EventsConsumer.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<OperationsDal>()
    .AddTransient<PositionsDal>()
    .AddTransient<AssetsDal>()
    .AddTransient<ClientServices>()
    .AddTransient<OperationsService>()
    .AddTransient<PositionsService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();