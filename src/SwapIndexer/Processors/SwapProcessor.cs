using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using SwapIndexer;
using SwapIndexer.Entities;

namespace SwapIndexer.Processors;

public class SwapProcessor : SwapProcessorBase<Awaken.Contracts.Swap.Swap>
{
    public override async Task ProcessAsync(Awaken.Contracts.Swap.Swap eventValue, LogEventContext context)
    {
        var indexId = IdGenerateHelper.GetId(context.ChainId, context.Transaction.TransactionId, context.Block.BlockHeight);
        var record = await GetEntityAsync<SwapRecordIndex>(indexId);
        if (record == null || record.TransactionHash.IsNullOrWhiteSpace())
        {
            record ??= new SwapRecordIndex
            {
                Id = indexId,
                Sender = eventValue.Sender.ToBase58()
            };
            record.Metadata.ChainId = context.ChainId;
            record.PairAddress = eventValue.Pair.ToBase58();
            record.TransactionHash = context.Transaction.TransactionId;
            record.Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(context.Block.BlockTime);
            record.AmountOut = eventValue.AmountOut;
            record.AmountIn = eventValue.AmountIn;
            record.TotalFee = eventValue.TotalFee;
            record.SymbolIn = eventValue.SymbolIn;
            record.SymbolOut = eventValue.SymbolOut;
            record.Channel = eventValue.Channel;
        }
        else
        {
            if (record.PairAddress == eventValue.Pair.ToBase58() || 
                record.SwapRecords.FirstOrDefault(s => s.PairAddress == eventValue.Pair.ToBase58()) != null)
            {
                return;
            }

            var swapRecord = new SwapRecord
            {
                AmountIn = eventValue.AmountIn,
                AmountOut = eventValue.AmountOut,
                SymbolIn = eventValue.SymbolIn,
                SymbolOut = eventValue.SymbolOut,
                TotalFee = eventValue.TotalFee,
                PairAddress = eventValue.Pair.ToBase58(),
                Channel = eventValue.Channel
            };
            record.SwapRecords.Add(swapRecord);
        }
        
        await SaveEntityAsync(record);
    }
}

public class SwapProcessor2 : SwapProcessor
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

public class SwapProcessor3 : SwapProcessor
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

public class SwapProcessor4 : SwapProcessor
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

public class SwapProcessor5 : SwapProcessor
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