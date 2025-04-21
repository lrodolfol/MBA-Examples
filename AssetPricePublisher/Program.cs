using AssetPricePublisher;
using AssetPricePublisher.ModelServices;
using Core.Configurations;
using Core.DAL.Abstractions;
using Core.DAL.Mysql;

var builder = Host.CreateApplicationBuilder(args);

MyConfigurations.LoadPropertiesFromEnvironmentVariables();

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

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();