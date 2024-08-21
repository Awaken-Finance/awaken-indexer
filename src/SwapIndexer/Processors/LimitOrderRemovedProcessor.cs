using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Order;
using SwapIndexer.Entities;
using SwapIndexer.Providers;
using SwapIndexer;

namespace SwapIndexer.Processors;

public class LimitOrderRemovedProcessor : LimitOrderProcessorBase<LimitOrderRemoved>
{
    public LimitOrderRemovedProcessor(
        IAElfDataProvider aelfDataProvider) : base(aelfDataProvider)
    {
    }
    
    public override async Task ProcessAsync(LimitOrderRemoved eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.OrderId);
        var recordIndex = await GetEntityAsync<LimitOrderIndex>(id);
        if (recordIndex == null)
        {
            Logger.LogInformation($"LimitOrderRemoved failed, can not find order id: {id}");
            return;
        }

        recordIndex.RemoveTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.RemoveTime.ToDateTime());
        recordIndex.LastUpdateTime = recordIndex.RemoveTime;
        recordIndex.LimitOrderStatus = eventValue.ReasonType == ReasonType.Expired
            ? LimitOrderStatus.Expired
            : LimitOrderStatus.Revoked;
        
        recordIndex.FillRecords.Add(new FillRecord()
        {
            TransactionTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.RemoveTime.ToDateTime()),
            TransactionHash = context.Transaction.TransactionId,
            Status = recordIndex.LimitOrderStatus
        });
        await SaveEntityAsync(recordIndex);
    }
}
