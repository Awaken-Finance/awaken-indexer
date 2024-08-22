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
        Logger.Info("received Swap:" + context.BlockTime + ",txn id:" + context.TransactionId + ",event:" + eventValue);
        var indexId = IdGenerateHelper.GetId(context.ChainId, context.TransactionId, context.BlockHeight);
        var record = await SwapRecordIndexRepository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        if (record == null || record.TransactionHash.IsNullOrWhiteSpace())
        {
            record ??= new SwapRecordIndex
            {
                Id = indexId,
                Sender = eventValue.Sender.ToBase58()
            };
            record.ChainId = context.ChainId;
            record.PairAddress = eventValue.Pair.ToBase58();
            record.TransactionHash = context.TransactionId;
            record.Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(context.BlockTime);
            record.AmountOut = eventValue.AmountOut;
            record.AmountIn = eventValue.AmountIn;
            record.TotalFee = eventValue.TotalFee;
            record.SymbolIn = eventValue.SymbolIn;
            record.SymbolOut = eventValue.SymbolOut;
            record.Channel = eventValue.Channel;
        }
        else
        {
            if (record.PairAddress == eventValue.Pair.ToBase58() || 
                record.SwapRecords.FirstOrDefault(s => s.PairAddress == eventValue.Pair.ToBase58()) != null)
            {
                return;
            }

            var swapRecord = new SwapRecord
            {
                AmountIn = eventValue.AmountIn,
                AmountOut = eventValue.AmountOut,
                SymbolIn = eventValue.SymbolIn,
                SymbolOut = eventValue.SymbolOut,
                TotalFee = eventValue.TotalFee,
                PairAddress = eventValue.Pair.ToBase58(),
                Channel = eventValue.Channel
            };
            record.SwapRecords.Add(swapRecord);
        }
        ObjectMapper.Map(context, record);
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