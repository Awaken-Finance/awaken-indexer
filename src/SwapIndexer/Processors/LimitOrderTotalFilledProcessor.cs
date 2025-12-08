using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Order;
using SwapIndexer.Entities;
using SwapIndexer.Providers;
using SwapIndexer;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class LimitOrderTotalFilledProcessor : LimitOrderProcessorBase<LimitOrderTotalFilled>
{
    public LimitOrderTotalFilledProcessor(
        IAElfDataProvider aelfDataProvider) : base(aelfDataProvider)
    {
    }
    
    public override async Task ProcessAsync(LimitOrderTotalFilled eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, context.Transaction.TransactionId, context.Block.BlockHeight);
        var record = await GetEntityAsync<SwapRecordIndex>(indexId);
        var swapSymbolIn = eventValue.SymbolOut;
        var swapSymbolOut = eventValue.SymbolIn;
        var swapAmountIn = eventValue.AmountOutFilled;
        var swapAmountOut = eventValue.AmountInFilled;
        if (record == null || record.TransactionHash.IsNullOrWhiteSpace())
        {
            record ??= new SwapRecordIndex
            {
                Id = indexId,
                Sender = eventValue.Sender.ToBase58()
            };
            record.Metadata.ChainId = context.ChainId;
            record.TransactionHash = context.Transaction.TransactionId;
            record.Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(context.Block.BlockTime);
            record.AmountOut = swapAmountOut;
            record.AmountIn = swapAmountIn;
            record.SymbolIn = swapSymbolIn;
            record.SymbolOut = swapSymbolOut;
            record.IsLimitOrder = true;
        }
        else
        {
            var swapRecord = new SwapRecord
            {
                AmountIn = swapAmountIn,
                AmountOut = swapAmountOut,
                SymbolIn = swapSymbolIn,
                SymbolOut = swapSymbolOut,
                IsLimitOrder = true
            };
            record.SwapRecords.Add(swapRecord);
        }
        
        await SaveEntityAsync(record);
    }
}
