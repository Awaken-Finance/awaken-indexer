using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElf.Types;
using Awaken.Contracts.Swap;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
using SwapIndexer.Application.Contracts.Token;
using SwapIndexer.Entities;
using SwapIndexer.GraphQL;
using SwapIndexer.Processors;
using SwapIndexer.Providers;
using SwapIndexer;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace SwapIndexer.Tests.Processors;

public class PairCreatedProcessorSub : PairCreatedProcessor
{
    public PairCreatedProcessorSub(
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(tradePairTokenOrderProvider)
    {
    }

    public async Task NewHandleEventAsync(PairCreated eventValue, LogEventContext context)
    {
        await base.ProcessAsync(eventValue,context);
    }
}

public class PairCreatedProcessorTests : SwapIndexerTestBase
{
    private readonly PairCreatedProcessor _pairCreatedEventProcessor;
    private readonly IReadOnlyRepository<TradePairInfoIndex> _tradepairRepository;
    const string ChainId = "AELF";
    
    public PairCreatedProcessorTests()
    {
        _pairCreatedEventProcessor = GetRequiredService<PairCreatedProcessor>();
        _tradepairRepository = GetRequiredService<IReadOnlyRepository<TradePairInfoIndex>>();
    }

    [Fact]
    public async void PairCreatedAsyncTest()
    {
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        var pairAddress = Address.FromPublicKey("AAA".HexToByteArray());
        var pairCreated = new PairCreated()
        {
            Pair = pairAddress,
            SymbolA = "USDT",
            SymbolB = "ELF"
        };
        var logEventContext = GenerateLogEventContext(pairCreated);
        logEventContext.Transaction.TransactionId = transactionId;
        
        await _pairCreatedEventProcessor.ProcessAsync(pairCreated, logEventContext);
        
        var queryable = await _tradepairRepository.GetQueryableAsync();
        queryable = queryable.Where(a => a.Address == pairAddress.ToBase58());
        
        var indexData =  queryable.ToList()[0];
        
        indexData.Address.ShouldBe(pairAddress.ToBase58());
        indexData.TransactionHash.ShouldBe(transactionId);
        indexData.Metadata.ChainId.ShouldBe(AwakenSwapConst.AELF);
        indexData.Token0Symbol.ShouldBe("ELF");
        indexData.Token1Symbol.ShouldBe("USDT");
        indexData.Metadata.ChainId.ShouldBe(ChainId);
        indexData.Metadata.Block.BlockHeight.ShouldBe(logEventContext.Block.BlockHeight);
        indexData.Metadata.Block.BlockHash.ShouldBe(logEventContext.Block.BlockHash);
    }
    
}