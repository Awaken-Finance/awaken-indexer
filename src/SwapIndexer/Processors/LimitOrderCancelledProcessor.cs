using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Order;
using Newtonsoft.Json;
using SwapIndexer.Entities;
using SwapIndexer.Providers;
using SwapIndexer;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class LimitOrderCancelledProcessor : LimitOrderProcessorBase<LimitOrderCancelled>
{
    public LimitOrderCancelledProcessor(
        IObjectMapper objectMapper,
        IAElfDataProvider aelfDataProvider) : base(aelfDataProvider)
    {
    }
    
    public override async Task ProcessAsync(LimitOrderCancelled eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.OrderId);
        var recordIndex = await GetEntityAsync<LimitOrderIndex>(id);
        if (recordIndex == null)
        {
            Logger.LogInformation($"LimitOrderCancelled failed, can not find order id: {id}");
            return;
        }

        var transactionFee = 0d;
        
        recordIndex.CancelTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.CancelTime.ToDateTime());
        recordIndex.LastUpdateTime = recordIndex.CancelTime;
        recordIndex.LimitOrderStatus = LimitOrderStatus.Cancelled;
        recordIndex.FillRecords.Add(new FillRecord()
        {
            TransactionTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.CancelTime.ToDateTime()),
            TransactionHash = context.Transaction.TransactionId,
            Status = LimitOrderStatus.Cancelled,
            TransactionFee = await GetTransactionFeeAsync(context.Transaction)
        });
        
        await SaveEntityAsync(recordIndex);
    }
}
