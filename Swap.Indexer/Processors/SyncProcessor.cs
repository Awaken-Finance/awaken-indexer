using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Swap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class SyncProcessor : SyncProcessorBase<Sync>
{
    public SyncProcessor(ILogger<SyncProcessor> logger, 
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions, 
        IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> syncRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, syncRecordIndexRepository)
    {
    }

    protected override async Task HandleEventAsync(Sync eventValue, LogEventContext context)
    {
        Logger.Info("received Sync:" + context.BlockTime);
        var record = new SyncRecordIndex
        {
            ChainId = context.ChainId,
            PairAddress = eventValue.Pair.ToBase58(),
            SymbolA = eventValue.SymbolA,
            SymbolB = eventValue.SymbolB,
            ReserveA = eventValue.ReserveA,
            ReserveB = eventValue.ReserveB,
            Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(context.BlockTime),
            TransactionHash = context.TransactionId
        };
        ObjectMapper.Map(eventValue, record);
        ObjectMapper.Map(context, record);
        record.Id = IdGenerateHelper.GetId(context.ChainId, context.TransactionId, eventValue.Pair.ToBase58());
        
        await SyncRecordIndexRepository.AddOrUpdateAsync(record);
    }
}

public class SyncProcessor2 : SyncProcessor
{
    public SyncProcessor2(ILogger<SyncProcessor2> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> syncRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, syncRecordIndexRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 2).SwapContractAddress;
    }
}

public class SyncProcessor3 : SyncProcessor
{
    public SyncProcessor3(ILogger<SyncProcessor3> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> syncRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, syncRecordIndexRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 3).SwapContractAddress;
    }
}

public class SyncProcessor4 : SyncProcessor
{
    public SyncProcessor4(ILogger<SyncProcessor4> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> syncRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, syncRecordIndexRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 4).SwapContractAddress;
    }
}

public class SyncProcessor5 : SyncProcessor
{
    public SyncProcessor5(ILogger<SyncProcessor5> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> syncRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, syncRecordIndexRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 5).SwapContractAddress;
    }
}