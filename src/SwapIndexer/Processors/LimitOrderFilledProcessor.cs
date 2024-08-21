using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Order;
using SwapIndexer.Entities;
using SwapIndexer.Providers;
using SwapIndexer;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class LimitOrderFilledProcessor : LimitOrderProcessorBase<LimitOrderFilled>
{
    public LimitOrderFilledProcessor(
        IObjectMapper objectMapper,
        IAElfDataProvider aelfDataProvider) : base(aelfDataProvider)
    {
    }
    
    public override async Task ProcessAsync(LimitOrderFilled eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.OrderId);
        var recordIndex = await GetEntityAsync<LimitOrderIndex>(id);
        if (recordIndex == null)
        {
            Logger.LogInformation($"LimitOrderFilled failed, can not find order id: {id}");
            return;
        }

        recordIndex.LimitOrderStatus = LimitOrderStatus.PartiallyFilling;
        recordIndex.FillTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.FillTime.ToDateTime());
        recordIndex.LastUpdateTime = recordIndex.FillTime;
        recordIndex.AmountInFilled += eventValue.AmountInFilled;
        recordIndex.AmountOutFilled += eventValue.AmountOutFilled;
        var transactionFee = 0L;
        var takerAddress = eventValue.Taker.ToBase58();
        if (takerAddress == recordIndex.Maker)
        {
            transactionFee = await GetTransactionFeeAsync(context.Transaction);
        }
        recordIndex.FillRecords.Add(new FillRecord()
        {
            AmountInFilled = eventValue.AmountInFilled,
            AmountOutFilled = eventValue.AmountOutFilled,
            TransactionTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.FillTime.ToDateTime()),
            TakerAddress = takerAddress,
            TransactionHash = context.Transaction.TransactionId,
            Status = LimitOrderStatus.PartiallyFilling,
            TotalFee = eventValue.TotalFee,
            TransactionFee = transactionFee
        });
        
        if ((recordIndex.AmountIn > 0 && recordIndex.AmountIn == recordIndex.AmountInFilled) || 
            (recordIndex.AmountOut > 0 && recordIndex.AmountOut == recordIndex.AmountOutFilled))
        {
            recordIndex.LimitOrderStatus = LimitOrderStatus.FullFilled;
        }
        await SaveEntityAsync(recordIndex);
    }
}
