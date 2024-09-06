using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Order;
using Awaken.Contracts.Swap;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Swap.Indexer.Providers;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class LimitOrderCreatedProcessor : LimitOrderProcessorBase<LimitOrderCreated>
{
    public LimitOrderCreatedProcessor(
        ILogger<LimitOrderCreatedProcessor> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfDataProvider aelfDataProvider) : base(logger,objectMapper, contractInfoOptions, repository, aelfDataProvider)
    {
    }
    
    protected override async Task HandleEventAsync(LimitOrderCreated eventValue, LogEventContext context)
    {
        Logger.Info("received LimitOrderCreated:" + eventValue + ",context:" + context);
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.OrderId);
        var recordIndex = await Repository.GetFromBlockStateSetAsync(id, context.ChainId) ?? new LimitOrderIndex()
        {
            Id = id
        };
        
        ObjectMapper.Map(eventValue, recordIndex);
        recordIndex.LimitOrderStatus = LimitOrderStatus.Committed;
        recordIndex.TransactionHash = context.TransactionId;
        recordIndex.LastUpdateTime = recordIndex.CommitTime;
        recordIndex.TransactionFee = await AElfDataProvider.GetTransactionFeeAsync(context.ChainId, context.TransactionId);
        ObjectMapper.Map(context, recordIndex);
        Logger.Info("LimitOrderIndex:" + recordIndex);
        await Repository.AddOrUpdateAsync(recordIndex);
    }
}