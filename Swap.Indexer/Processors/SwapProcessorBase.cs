using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class SwapProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo>
    where TEvent : IEvent<TEvent>,new()
{
    protected readonly ILogger<SwapProcessorBase<TEvent>> Logger;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> Repository;
    protected readonly IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> SwapRecordIndexRepository;
    public SwapProcessorBase(ILogger<SwapProcessorBase<TEvent>> logger, 
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository) : base(logger)
    {
        Logger = logger;
        ObjectMapper = objectMapper;
        ContractInfoOptions = contractInfoOptions.Value;
        Repository = repository;
        SwapRecordIndexRepository = swapRecordIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 1).SwapContractAddress;
    }

    protected virtual double GetContractFeeRate(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 1).FeeRate;
    }
}