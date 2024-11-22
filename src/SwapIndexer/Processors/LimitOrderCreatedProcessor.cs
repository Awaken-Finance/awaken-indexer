using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Order;
using SwapIndexer.Entities;

using SwapIndexer.Providers;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class LimitOrderCreatedProcessor : LimitOrderProcessorBase<LimitOrderCreated>
{
    public LimitOrderCreatedProcessor(
        IObjectMapper objectMapper,
        IAElfDataProvider aelfDataProvider) : base(aelfDataProvider)
    {
    }
    
    public override async Task ProcessAsync(LimitOrderCreated eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.OrderId);
        var recordIndex = await GetEntityAsync<LimitOrderIndex>(id) ?? new LimitOrderIndex()
        {
            Id = id
        };
        
        ObjectMapper.Map(eventValue, recordIndex);
        recordIndex.LimitOrderStatus = LimitOrderStatus.Committed;
        recordIndex.TransactionHash = context.Transaction.TransactionId;
        recordIndex.LastUpdateTime = recordIndex.CommitTime;
        recordIndex.TransactionFee = await GetTransactionFeeAsync(context.Transaction);
        await SaveEntityAsync(recordIndex);
    }
}
