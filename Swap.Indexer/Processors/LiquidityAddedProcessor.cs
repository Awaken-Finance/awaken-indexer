using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Swap;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class LiquidityAddedProcessor : LiquidityProcessorBase<LiquidityAdded>
{
    public LiquidityAddedProcessor(
        ILogger<LiquidityAddedProcessor> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> userLiquidityRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger,objectMapper, contractInfoOptions, repository, userLiquidityRepository)
    {
    }
    
    protected override async Task HandleEventAsync(LiquidityAdded eventValue, LogEventContext context)
    {
        Logger.Info("received LiquidityAdded:" + eventValue + ",context:" + context);
        var id = IdGenerateHelper.GetId(context.ChainId, context.TransactionId);
        var recordIndex = await Repository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (recordIndex != null)
        {
            return;
        }

        recordIndex = new LiquidityRecordIndex();
        ObjectMapper.Map(eventValue, recordIndex);
        ObjectMapper.Map(context, recordIndex);
        
        recordIndex.Id = id;
        recordIndex.Type = LiquidityRecordIndex.LiquidityType.Mint;
        recordIndex.Sender = eventValue.Sender.ToBase58();
        recordIndex.To = eventValue.To.ToBase58();
        recordIndex.Pair = eventValue.Pair.ToBase58();
        recordIndex.Address = eventValue.To.ToBase58();
        recordIndex.Token0Amount = eventValue.AmountA;
        recordIndex.Token1Amount = eventValue.AmountB;
        recordIndex.LpTokenAmount = eventValue.LiquidityToken;
        recordIndex.Token0 = eventValue.SymbolA;
        recordIndex.Token1 = eventValue.SymbolB;
        recordIndex.Timestamp = context.BlockTime.ToTimestamp().Seconds * 1000 + context.BlockTime.Millisecond;
        recordIndex.TransactionHash = context.TransactionId;
        
        Logger.Info("LiquidityRecordIndex:" + recordIndex);
        await Repository.AddOrUpdateAsync(recordIndex);
        await HandlerUserLiquidityAsync(recordIndex, context);
    }
}

public class LiquidityAddedProcessor2 : LiquidityAddedProcessor
{
    public LiquidityAddedProcessor2(
        ILogger<LiquidityAddedProcessor2> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> userLiquidityRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper,
        repository, userLiquidityRepository, contractInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 2).SwapContractAddress;
    }
}

public class LiquidityAddedProcessor3 : LiquidityAddedProcessor
{
    public LiquidityAddedProcessor3(
        ILogger<LiquidityAddedProcessor3> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> userLiquidityRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper,
        repository, userLiquidityRepository, contractInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 3).SwapContractAddress;
    }
}

public class LiquidityAddedProcessor4 : LiquidityAddedProcessor
{
    public LiquidityAddedProcessor4(
        ILogger<LiquidityAddedProcessor4> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> userLiquidityRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper,
        repository, userLiquidityRepository, contractInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 4).SwapContractAddress;
    }
}

public class LiquidityAddedProcessor5 : LiquidityAddedProcessor
{
    public LiquidityAddedProcessor5(
        ILogger<LiquidityAddedProcessor5> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> userLiquidityRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger, objectMapper,
        repository, userLiquidityRepository, contractInfoOptions)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 5).SwapContractAddress;
    }
}