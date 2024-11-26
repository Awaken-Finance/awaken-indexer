using System.Reflection;
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
    private readonly IObjectMapper _objectMapper;

    const string ChainId = "AELF";
    
    public PairCreatedProcessorTests()
    {
        _pairCreatedEventProcessor = GetRequiredService<PairCreatedProcessor>();
        _tradepairRepository = GetRequiredService<IReadOnlyRepository<TradePairInfoIndex>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }
    
    [Fact]
    public async Task PairCreatedProcessorContractAddressAsyncTests()
    {
        _pairCreatedEventProcessor.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("2YnkipJ9mty5r6tpTWQAwnomeeKUT7qCWLHKaSeV1fejYEyCdX");
        _pairCreatedEventProcessor.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("hyiwdsbDnyoG1uZiw2JabQ4tLiWT6yAuDfNBFbHhCZwAqU1os");
        _pairCreatedEventProcessor.GetContractAddress("notexist").ShouldBe(string.Empty);
        Assert.Equal(_pairCreatedEventProcessor.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_pairCreatedEventProcessor, new object[] { AwakenSwapConst.tDVW }), 0.003);
        Assert.Equal(_pairCreatedEventProcessor.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_pairCreatedEventProcessor, new object[] { AwakenSwapConst.tDVV }), 0.001);
        Assert.Equal(_pairCreatedEventProcessor.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_pairCreatedEventProcessor, new object[] { "notexist" }), 0.0);
        
        var pairCreatedEventProcessor2 = GetRequiredService<PairCreatedProcessor2>();
        pairCreatedEventProcessor2.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("fGa81UPViGsVvTM13zuAAwk1QHovL3oSqTrCznitS4hAawPpk");
        pairCreatedEventProcessor2.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("JvDB3rguLJtpFsovre8udJeXJLhsV1EPScGz2u1FFneahjBQm");
        pairCreatedEventProcessor2.GetContractAddress("notexist").ShouldBe(string.Empty);
        Assert.Equal(pairCreatedEventProcessor2.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor2, new object[] { AwakenSwapConst.tDVW }), 0.0005);
        Assert.Equal(pairCreatedEventProcessor2.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor2, new object[] { AwakenSwapConst.tDVV }), 0.003);
        Assert.Equal(pairCreatedEventProcessor2.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor2, new object[] { "notexist" }), 0.0);
        
        
        var pairCreatedEventProcessor3 = GetRequiredService<PairCreatedProcessor3>();
        pairCreatedEventProcessor3.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("LzkrbEK2zweeuE4P8Y23BMiFY2oiKMWyHuy5hBBbF1pAPD2hh");
        pairCreatedEventProcessor3.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("83ju3fGGnvQzCmtjApUTwvBpuLQLQvt5biNMv4FXCvWKdZgJf");
        pairCreatedEventProcessor3.GetContractAddress("notexist").ShouldBe(string.Empty);
        Assert.Equal(pairCreatedEventProcessor3.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor3, new object[] { AwakenSwapConst.tDVW }), 0.001);
        Assert.Equal(pairCreatedEventProcessor3.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor3, new object[] { AwakenSwapConst.tDVV }), 0.0005);
        Assert.Equal(pairCreatedEventProcessor3.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor3, new object[] { "notexist" }), 0.0);
        
        var pairCreatedEventProcessor4 = GetRequiredService<PairCreatedProcessor4>();
        pairCreatedEventProcessor4.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("EG73zzQqC8JencoFEgCtrEUvMBS2zT22xoRse72XkyhuuhyTC");
        pairCreatedEventProcessor4.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("2q7NLAr6eqF4CTsnNeXnBZ9k4XcmiUeM61CLWYaym6WsUmbg1k");
        pairCreatedEventProcessor4.GetContractAddress("notexist").ShouldBe(string.Empty);
        Assert.Equal(pairCreatedEventProcessor4.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor4, new object[] { AwakenSwapConst.tDVW }), 0.03);
        Assert.Equal(pairCreatedEventProcessor4.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor4, new object[] { AwakenSwapConst.tDVV }), 0.03);
        Assert.Equal(pairCreatedEventProcessor4.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor4, new object[] { "notexist" }), 0.0);
        
        var pairCreatedEventProcessor5 = GetRequiredService<PairCreatedProcessor5>();
        pairCreatedEventProcessor5.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("23dh2s1mXnswi4yNW7eWNKWy7iac8KrXJYitECgUctgfwjeZwP");
        pairCreatedEventProcessor5.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("UYdd84gLMsVdHrgkr3ogqe1ukhKwen8oj32Ks4J1dg6KH9PYC");
        pairCreatedEventProcessor5.GetContractAddress("notexist").ShouldBe(string.Empty);
        Assert.Equal(pairCreatedEventProcessor5.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor5, new object[] { AwakenSwapConst.tDVW }), 0.05);
        Assert.Equal(pairCreatedEventProcessor5.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor5, new object[] { AwakenSwapConst.tDVV }), 0.05);
        Assert.Equal(pairCreatedEventProcessor5.GetType()
            .GetMethod("GetContractFeeRate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pairCreatedEventProcessor5, new object[] { "notexist" }), 0.0);
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
        logEventContext.Block.BlockHeight = 100;
        
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
        
        var result = await Query.TradePairInfoAsync(_tradepairRepository, _objectMapper, new GetTradePairInfoDto
        {
            ChainId = AwakenSwapConst.AELF,
            Token0Symbol = "ELF",
            Token1Symbol = "USDT",
            SkipCount = 0,
            MaxResultCount = 10
        });
        result.Count.ShouldBe(1);
        result[0].Token0Symbol.ShouldBe("ELF");
        result[0].Token1Symbol.ShouldBe("USDT");
        result[0].Address.ShouldBe(pairAddress.ToBase58());
        
        var getListResult = await Query.GetTradePairInfoListAsync(_tradepairRepository, _objectMapper, new GetTradePairInfoDto
        {
            ChainId = AwakenSwapConst.AELF,
            SkipCount = 0,
            MaxResultCount = 10,
            StartBlockHeight = 100,
            EndBlockHeight = 101,
            Address = pairAddress.ToBase58(),
            Token0Symbol = "ELF",
            Token1Symbol = "USDT",
            TokenSymbol = "ELF",
            Id = indexData.Id
        });
        getListResult.TotalCount.ShouldBe(1);
        getListResult.Data[0].Token0Symbol.ShouldBe("ELF");
        getListResult.Data[0].Token1Symbol.ShouldBe("USDT");
        getListResult.Data[0].Address.ShouldBe(pairAddress.ToBase58());
    }
    
}