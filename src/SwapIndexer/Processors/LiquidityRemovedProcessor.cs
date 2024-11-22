using AeFinder.Sdk.Logging;
using Awaken.Contracts.Swap;
using AeFinder.Sdk.Processor;
using SwapIndexer;
using Google.Protobuf.WellKnownTypes;
using SwapIndexer.Entities;

namespace SwapIndexer.Processors;

public class LiquidityRemovedProcessor : LiquidityProcessorBase<LiquidityRemoved>
{
   
    public override async Task ProcessAsync(LiquidityRemoved eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, context.Transaction.TransactionId);
        var recordIndex = await GetEntityAsync<LiquidityRecordIndex>(id);
        if (recordIndex != null)
        {
            return;
        }
        recordIndex = new LiquidityRecordIndex();

        recordIndex.Id = id;
        recordIndex.Type = LiquidityRecordIndex.LiquidityType.Burn;
        recordIndex.Sender = eventValue.Sender.ToBase58();
        recordIndex.To = eventValue.To.ToBase58();
        recordIndex.Pair = eventValue.Pair.ToBase58();
        recordIndex.Address = eventValue.To.ToBase58();
        recordIndex.Token0Amount = eventValue.AmountA;
        recordIndex.Token1Amount = eventValue.AmountB;
        recordIndex.LpTokenAmount = eventValue.LiquidityToken;
        recordIndex.Token0 = eventValue.SymbolA;
        recordIndex.Token1 = eventValue.SymbolB;
        recordIndex.Timestamp = context.Block.BlockTime.ToTimestamp().Seconds * 1000 + context.Block.BlockTime.Millisecond;
        recordIndex.TransactionHash = context.Transaction.TransactionId;
        await SaveEntityAsync(recordIndex);
        await HandlerUserLiquidityAsync(recordIndex, context);
    }
}

public class LiquidityRemovedProcessor2 : LiquidityRemovedProcessor
{
    public override string GetContractAddress(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel2,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel2,
            _ => string.Empty
        };
    }
}

public class LiquidityRemovedProcessor3 : LiquidityRemovedProcessor
{
    public override string GetContractAddress(string chainId)
    {
        
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel3,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel3,
            _ => string.Empty
        };
    }
}

public class LiquidityRemovedProcessor4 : LiquidityRemovedProcessor
{
    public override string GetContractAddress(string chainId)
    {
        
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel4,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel4,
            _ => string.Empty
        };
    }
}

public class LiquidityRemovedProcessor5 : LiquidityRemovedProcessor
{
    public override string GetContractAddress(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel5,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel5,
            _ => string.Empty
        };
    }
}