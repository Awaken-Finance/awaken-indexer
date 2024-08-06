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

public abstract class LimitOrderProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo> where TEvent : IEvent<TEvent>, new()
{
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> Repository;
    protected readonly ILogger<AElfLogEventProcessorBase<TEvent, LogEventInfo>> Logger;
    protected LimitOrderProcessorBase(
        ILogger<AElfLogEventProcessorBase<TEvent, LogEventInfo>> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository) : base(logger)
    {
        Logger = logger;
        ObjectMapper = objectMapper;
        ContractInfoOptions = contractInfoOptions.Value;
        Repository = repository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 1 && !string.IsNullOrEmpty(o.LimitOrderContractAddress)).LimitOrderContractAddress;
    }
}