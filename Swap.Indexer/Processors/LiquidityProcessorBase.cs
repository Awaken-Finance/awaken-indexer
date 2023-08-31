using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public abstract class LiquidityProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo> where TEvent : IEvent<TEvent>, new()
{
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> Repository;
    protected readonly IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> UserLiquidityRepository;
    protected readonly ILogger<AElfLogEventProcessorBase<TEvent, LogEventInfo>> Logger;
    protected LiquidityProcessorBase(
        ILogger<AElfLogEventProcessorBase<TEvent, LogEventInfo>> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> userLiquidityRepository) : base(logger)
    {
        Logger = logger;
        ObjectMapper = objectMapper;
        ContractInfoOptions = contractInfoOptions.Value;
        Repository = repository;
        UserLiquidityRepository = userLiquidityRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 1).SwapContractAddress;
    }

    protected async Task HandlerUserLiquidityAsync(LiquidityRecordIndex recordIndex, LogEventContext context)
    {
        var userLiquidityId = IdGenerateHelper.GetId(recordIndex.ChainId, recordIndex.Address, recordIndex.Pair);
        var userLiquidity = await UserLiquidityRepository.GetFromBlockStateSetAsync(userLiquidityId, recordIndex.ChainId);
        if (userLiquidity == null)
        {
            userLiquidity = new UserLiquidityIndex();
            ObjectMapper.Map(recordIndex, userLiquidity);
            userLiquidity.Id = userLiquidityId;
        }
        else
        {
            if (recordIndex.Type == LiquidityRecordIndex.LiquidityType.Mint)
            {
                userLiquidity.LpTokenAmount += recordIndex.LpTokenAmount;
            }
            else
            {
                userLiquidity.LpTokenAmount -= recordIndex.LpTokenAmount;
            }
        }
        ObjectMapper.Map(context, userLiquidity);
        Logger.Info("UserLiquidityIndex:" + recordIndex);
        await UserLiquidityRepository.AddOrUpdateAsync(userLiquidity);
    }
}