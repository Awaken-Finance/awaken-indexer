using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Swap;
using SwapIndexer;
using Google.Protobuf.WellKnownTypes;
using SwapIndexer.Entities;

namespace SwapIndexer.Processors;

public class LiquidityAddedProcessor : LiquidityProcessorBase<LiquidityAdded>
{
   
    public override async Task ProcessAsync(LiquidityAdded eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, context.Transaction.TransactionId);
        var recordIndex = await GetEntityAsync<LiquidityRecordIndex>(id);
        
        if (recordIndex != null)
        {
            return;
        }

        recordIndex = new LiquidityRecordIndex();
        ObjectMapper.Map(eventValue, recordIndex);
        
        recordIndex.Id = id;
        recordIndex.Type = LiquidityRecordIndex.LiquidityType.Mint;
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

public class LiquidityAddedProcessor2 : LiquidityAddedProcessor
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

public class LiquidityAddedProcessor3 : LiquidityAddedProcessor
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

public class LiquidityAddedProcessor4 : LiquidityAddedProcessor
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

public class LiquidityAddedProcessor5 : LiquidityAddedProcessor
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