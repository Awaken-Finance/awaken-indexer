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

public abstract class SyncProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent,LogEventInfo>
    where TEvent : IEvent<TEvent>,new()
{
    protected readonly ILogger<SyncProcessorBase<TEvent>> Logger;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> SyncRecordIndexRepository;
    public SyncProcessorBase(ILogger<SyncProcessorBase<TEvent>> logger, 
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> syncRecordIndexRepository) : base(logger)
    {
        Logger = logger;
        ObjectMapper = objectMapper;
        ContractInfoOptions = contractInfoOptions.Value;
        SyncRecordIndexRepository = syncRecordIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 1).SwapContractAddress;
    }
}