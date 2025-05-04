using Core.Configurations;
using Core.DAL.Mysql;
using Core.Models.Agregates;
using Core.Models.Entities;
using EventsConsumer.Models.Enums;
using EventsConsumer.Models.Exceptions;

namespace EventsConsumer.Services;

public class BlockValidationService
{
    private readonly MyConfigurations.MysqlConfiguration _mysqlEnvironments = MyConfigurations.MysqlEnvironment;
    private readonly ILogger<ClientServices> _logger;

    public BlockValidationService(ILogger<ClientServices> logger)
    {
        _logger = logger;
    }
    
    public async Task<List<Blocks>> GetBlockIfExists((int clientId, int assetId) clientAndAssetId)
    {
        var blocks = new List<Blocks>();
        
        var dal = new BlockValidationDal(
            _mysqlEnvironments.Host,
            _mysqlEnvironments.UserName,
            _mysqlEnvironments.Password,
            _mysqlEnvironments.Database,
            _mysqlEnvironments.Port
        );

        if (clientAndAssetId.clientId <= 0 || clientAndAssetId.assetId <= 0)
            throw new InvalidClientIdException($"The cannot be less than zero");

        List<OperationsDailySummary> summaryOperations =
            await dal.GetTotalSumaryOperationsAsync(clientAndAssetId.clientId, clientAndAssetId.assetId);
        if (!dal.ResultTasks.IsSuccess)
        {
            _logger.LogError("The dal had a failure to get the summary operations. {Message}", dal.ResultTasks.ErrorMessage);
            return blocks;
        }
        
        List<Positions> summaryPositions =
            await dal.GetSummaryPositionsAsync(clientAndAssetId.clientId, clientAndAssetId.assetId);
        if (!dal.ResultTasks.IsSuccess)
        {
            _logger.LogError("The dal had a failure to get the summary positions. {Message}", dal.ResultTasks.ErrorMessage);
            return blocks;
        }

        blocks = LoadBlocks(summaryOperations, summaryPositions);
        return blocks;
    }

    private List<Blocks> LoadBlocks(List<OperationsDailySummary> summaryOperations, List<Positions> summaryPositions)
    {
        var blocks = new List<Blocks>();
        var blockDescriptionDicitionary = new Dictionary<string, string>();
        blockDescriptionDicitionary.Add("NOT-FOUND",
            Core.Helpers.Extensions.GetEnumDescription(BlockDescription.DailyPositionNotFound));
        blockDescriptionDicitionary.Add("LESS-THAN",
            Core.Helpers.Extensions.GetEnumDescription(BlockDescription.DailyOperationLessThanDailyPosition));
        blockDescriptionDicitionary.Add("GREATER-THAN",
            Core.Helpers.Extensions.GetEnumDescription(BlockDescription.DailyOperationGreaterThanDailyPosition));

        foreach (var summaryOper in summaryOperations)
        {
            var summaryDaily = summaryPositions.Find(x =>
                x.ClientId.ToString() == summaryOper.ClientIdAssetId.Split("-")[0]
                && x.AssetId.ToString() == summaryOper.ClientIdAssetId.Split("-")[1]
                && x.Date == summaryOper.OperationDate
            );

            if (summaryDaily is null)
            {
                blocks.Add(
                    new Blocks(
                        Convert.ToInt16(summaryOper.ClientIdAssetId.Split("-")[0]),
                        Convert.ToInt16(summaryOper.ClientIdAssetId.Split("-")[1]),
                        blockDescriptionDicitionary["NOT-FOUND"])
                );

                _logger.LogWarning("New block generated: {ClientId} - {AssetId} - {Description}",
                    summaryOper.ClientIdAssetId.Split("-")[0],
                    summaryOper.ClientIdAssetId.Split("-")[1],
                    blockDescriptionDicitionary["NOT-FOUND"]);

                continue;
            }

            if (summaryOper.TotalAccrued > summaryDaily.Amount)
            {
                blocks.Add(
                    new Blocks(
                        Convert.ToInt16(summaryOper.ClientIdAssetId.Split("-")[0]),
                        Convert.ToInt16(summaryOper.ClientIdAssetId.Split("-")[1]),
                        blockDescriptionDicitionary["GREATER-THAN"])
                );

                _logger.LogWarning("New block generated: {ClientId} - {AssetId} - {Description}",
                    summaryOper.ClientIdAssetId.Split("-")[0],
                    summaryOper.ClientIdAssetId.Split("-")[1],
                    blockDescriptionDicitionary["GREATER-THAN"]);

                continue;
            }

            if (summaryOper.TotalAccrued < summaryDaily.Amount)
            {
                blocks.Add(
                    new Blocks(
                        Convert.ToInt16(summaryOper.ClientIdAssetId.Split("-")[0]),
                        Convert.ToInt16(summaryOper.ClientIdAssetId.Split("-")[1]),
                        blockDescriptionDicitionary["LESS-THAN"])
                );

                _logger.LogWarning("New block generated: {ClientId} - {AssetId} - {Description}",
                    summaryOper.ClientIdAssetId.Split("-")[0],
                    summaryOper.ClientIdAssetId.Split("-")[1],
                    blockDescriptionDicitionary["LESS-THAN"]);
            }
        }

        return blocks;
    }
    
    public async Task InsertBlockAsync(List<Blocks> blocks)
    {
        var dal = new BlockValidationDal(
            _mysqlEnvironments.Host,
            _mysqlEnvironments.UserName,
            _mysqlEnvironments.Password,
            _mysqlEnvironments.Database,
            _mysqlEnvironments.Port
        );

        await dal.InsertBlockAsync(blocks);
        if(!dal.ResultTasks.IsSuccess)
            _logger.LogError("The dal had a failure to insert the blocks. {Message}", dal.ResultTasks.ErrorMessage);
    }

    public async Task BlockWorkFlowAsync((int clientId, int assetId) clientAndAssetId)
    {
        var blocks = await GetBlockIfExists((clientAndAssetId.clientId, clientAndAssetId.assetId));
        if (blocks.Count <= 0)
            return;

        await InsertBlockAsync(blocks);
    }
}