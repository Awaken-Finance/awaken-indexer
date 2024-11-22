using AeFinder.Sdk.Logging;
using Awaken.Contracts.Swap;
using Volo.Abp.ObjectMapping;
using AeFinder.Sdk.Processor;

using Newtonsoft.Json;
using SwapIndexer;
using SwapIndexer.Entities;
using SwapIndexer.Providers;

namespace SwapIndexer.Processors;

public class PairCreatedProcessor : SwapProcessorBase<PairCreated>
{
    private readonly ITradePairTokenOrderProvider _tradePairTokenOrderProvider;
    
    public PairCreatedProcessor(
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
    {
        _tradePairTokenOrderProvider = tradePairTokenOrderProvider;
    }
    
    public override async Task ProcessAsync(PairCreated eventValue, LogEventContext context)
    {
        // Logger.LogInformation($"PairCreated begin, " +
        //                       $"txnId: {context.Transaction.TransactionId}, " +
        //                       $"SymbolA: {eventValue.SymbolA}, " +
        //                       $"SymbolB: {eventValue.SymbolB}, " +
        //                       $"ChainId: {context.ChainId}");
        
        var token0Weight =
            _tradePairTokenOrderProvider.GetTokenWeight(eventValue.SymbolA);
        var token1Weight =
            _tradePairTokenOrderProvider.GetTokenWeight(eventValue.SymbolB);
        var isTokenReversed = token0Weight > token1Weight;
        var info = new TradePairInfoIndex
        {
            Id = Guid.NewGuid().ToString(),
            Address = eventValue.Pair.ToBase58(),
            Token0Symbol = isTokenReversed ? eventValue.SymbolB : eventValue.SymbolA,
            Token1Symbol = isTokenReversed ? eventValue.SymbolA : eventValue.SymbolB, 
            FeeRate = GetContractFeeRate(context.ChainId),
            IsTokenReversed = isTokenReversed,
            TransactionHash = context.Transaction.TransactionId
        };
        
        await SaveEntityAsync(info);
        
        // Logger.LogInformation($"PairCreated end, " +
        //                       $"txnId: {context.Transaction.TransactionId}, " +
        //                       $"Token0Symbol: {info.Token0Symbol}, " +
        //                       $"Token1Symbol: {info.Token1Symbol}, " +
        //                       $"FeeRate: {info.FeeRate}, " +
        //                       $"Address: {info.Address}");
    }
}

public class PairCreatedProcessor2 : PairCreatedProcessor
{
    public PairCreatedProcessor2(
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(
            tradePairTokenOrderProvider)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel2,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel2,
            _ => string.Empty
        };
    }
    
    protected override double GetContractFeeRate(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractFeeRateTDVVLevel2,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractFeeRateTDVWLevel2,
            _ => 0.0
        };
    }
    
}

public class PairCreatedProcessor3 : PairCreatedProcessor
{
    public PairCreatedProcessor3(ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(tradePairTokenOrderProvider) {}
    
    public override string GetContractAddress(string chainId)
    {
        
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel3,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel3,
            _ => string.Empty
        };
    }
    
    protected override double GetContractFeeRate(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractFeeRateTDVVLevel3,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractFeeRateTDVWLevel3,
            _ => 0.0
        };
    }
}

public class PairCreatedProcessor4 : PairCreatedProcessor
{
    public PairCreatedProcessor4(
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base( 
            tradePairTokenOrderProvider)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel4,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel4,
            _ => string.Empty
        };
    }
    
    protected override double GetContractFeeRate(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractFeeRateTDVVLevel4,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractFeeRateTDVWLevel4,
            _ => 0.0
        };
    }
}

public class PairCreatedProcessor5 : PairCreatedProcessor
{
    public PairCreatedProcessor5(
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base( 
            tradePairTokenOrderProvider)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel5,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel5,
            _ => string.Empty
        };
    }
    
    protected override double GetContractFeeRate(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractFeeRateTDVVLevel5,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractFeeRateTDVWLevel5,
            _ => 0.0
        };
    }
}