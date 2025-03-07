﻿using Core;
using Core.Configurations;
using Core.DAL.Mysql;
using Microsoft.Extensions.Logging;

namespace EventsPublisher.Services;

public class AssetsServices
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    private readonly ILogger<AssetsServices> _logger;

    public AssetsServices(ILogger<AssetsServices> logger) => _logger = logger;

    public async Task<List<Assets>> GetAssetsAsync()
    {        
        var dal = new AssetsDal(
            _mysqlEnvironments.Host, 
            _mysqlEnvironments.UserName, 
            _mysqlEnvironments.Password, 
            _mysqlEnvironments.Database, 
            _mysqlEnvironments.Port    
        );
        
        var assets = await dal.GetAssetsAsync();
        if(! dal.ResultTaskDataBase.IsSuccess)
            _logger.LogError("Error for get assets -> {0}", dal.ResultTaskDataBase.ErrorMessage);
        
        return assets;
    }
}