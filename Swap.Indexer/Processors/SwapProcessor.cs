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

public class SwapProcessor : SwapProcessorBase<Awaken.Contracts.Swap.Swap>
{
    public SwapProcessor(ILogger<SwapProcessor> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository)
    {
    }

    protected override async Task HandleEventAsync(Awaken.Contracts.Swap.Swap eventValue, LogEventContext context)
    {
        Logger.Info("received Swap:" + context.BlockTime);
        var record = new SwapRecordIndex
        {
            ChainId = context.ChainId,
            PairAddress = eventValue.Pair.ToBase58(),
            Sender = eventValue.Sender.ToBase58(),
            TransactionHash = context.TransactionId,
            Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(context.BlockTime),
            AmountOut = eventValue.AmountOut,
            AmountIn = eventValue.AmountIn,
            TotalFee = eventValue.TotalFee,
            SymbolOut = eventValue.SymbolOut,
            SymbolIn = eventValue.SymbolIn,
            Channel = eventValue.Channel
        };
        ObjectMapper.Map(eventValue, record);
        ObjectMapper.Map(context, record);
        record.Id = IdGenerateHelper.GetId(context.ChainId, context.TransactionId, record.BlockHeight);
        record.Sender = eventValue.Sender.ToBase58();
        
        await SwapRecordIndexRepository.AddOrUpdateAsync(record);
    }
}

public class SwapProcessor2 : SwapProcessor
{
    public SwapProcessor2(ILogger<SwapProcessor2> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 2).SwapContractAddress;
    }
}

public class SwapProcessor3 : SwapProcessor
{
    public SwapProcessor3(ILogger<SwapProcessor3> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 3).SwapContractAddress;
    }
}

public class SwapProcessor4 : SwapProcessor
{
    public SwapProcessor4(ILogger<SwapProcessor4> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 4).SwapContractAddress;
    }
}

public class SwapProcessor5 : SwapProcessor
{
    public SwapProcessor5(ILogger<SwapProcessor5> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 5).SwapContractAddress;
    }
}