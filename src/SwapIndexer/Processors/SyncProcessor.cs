using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Swap;
using SwapIndexer;
using SwapIndexer.Entities;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class SyncProcessor : SyncProcessorBase<Sync>
{
    public override async Task ProcessAsync(Sync eventValue, LogEventContext context)
    {
        var record = new SyncRecordIndex
        {
            PairAddress = eventValue.Pair.ToBase58(),
            SymbolA = eventValue.SymbolA,
            SymbolB = eventValue.SymbolB,
            ReserveA = eventValue.ReserveA,
            ReserveB = eventValue.ReserveB,
            Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(context.Block.BlockTime),
            TransactionHash = context.Transaction.TransactionId
        };
        ObjectMapper.Map(eventValue, record);
        record.Id = IdGenerateHelper.GetId(context.ChainId, context.Transaction.TransactionId, eventValue.Pair.ToBase58());
        
        await SaveEntityAsync(record);
    }
}

public class SyncProcessor2 : SyncProcessor
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

public class SyncProcessor3 : SyncProcessor
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

public class SyncProcessor4 : SyncProcessor
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

public class SyncProcessor5 : SyncProcessor
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