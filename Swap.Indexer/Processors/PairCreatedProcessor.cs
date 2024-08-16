using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Swap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Swap.Indexer.Providers;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class PairCreatedProcessor : SwapProcessorBase<PairCreated>
{
    private readonly ITradePairTokenOrderProvider _tradePairTokenOrderProvider;
    public PairCreatedProcessor(ILogger<PairCreatedProcessor> logger, 
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions, 
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository,
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository)
    {
        _tradePairTokenOrderProvider = tradePairTokenOrderProvider;
    }
    
    protected override async Task HandleEventAsync(PairCreated eventValue, LogEventContext context)
    {
        var token0Weight =
            _tradePairTokenOrderProvider.GetTokenWeight(eventValue.SymbolA);
        var token1Weight =
            _tradePairTokenOrderProvider.GetTokenWeight(eventValue.SymbolB);
        var isTokenReversed = token0Weight > token1Weight;
        var info = new TradePairInfoIndex
        {
            Id = Guid.NewGuid().ToString(),
            Address = eventValue.Pair.ToBase58(),
            ChainId = context.ChainId,
            Token0Symbol = isTokenReversed ? eventValue.SymbolB : eventValue.SymbolA,
            Token1Symbol = isTokenReversed ? eventValue.SymbolA : eventValue.SymbolB, 
            FeeRate = GetContractFeeRate(context.ChainId),
            IsTokenReversed = isTokenReversed,
            TransactionHash = context.TransactionId
        };
        ObjectMapper.Map(context, info);
        ObjectMapper.Map(eventValue, info);
        await Repository.AddOrUpdateAsync(info);
    }
}

public class PairCreatedProcessor2 : PairCreatedProcessor
{
    public PairCreatedProcessor2(ILogger<PairCreatedProcessor2> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository,
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository,
            tradePairTokenOrderProvider)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 2).SwapContractAddress;
    }
    
    protected override double GetContractFeeRate(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 2).FeeRate;
    }
    
}

public class PairCreatedProcessor3 : PairCreatedProcessor
{
    public PairCreatedProcessor3(ILogger<PairCreatedProcessor3> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository,
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository,
            tradePairTokenOrderProvider)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 3).SwapContractAddress;
    }
    
    protected override double GetContractFeeRate(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 3).FeeRate;
    }
}

public class PairCreatedProcessor4 : PairCreatedProcessor
{
    public PairCreatedProcessor4(ILogger<PairCreatedProcessor4> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository,
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository,
            tradePairTokenOrderProvider)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 4).SwapContractAddress;
    }
    
    protected override double GetContractFeeRate(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 4).FeeRate;
    }
}

public class PairCreatedProcessor5 : PairCreatedProcessor
{
    public PairCreatedProcessor5(ILogger<PairCreatedProcessor5> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository,
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository,
            tradePairTokenOrderProvider)
    {
    }
    
    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 5).SwapContractAddress;
    }
    
    protected override double GetContractFeeRate(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 5).FeeRate;
    }
}