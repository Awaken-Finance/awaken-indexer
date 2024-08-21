using AeFinder.Sdk;
using AElf.Types;
using Awaken.Contracts.Swap;
using Google.Protobuf.WellKnownTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
using SwapIndexer.Entities;
using SwapIndexer.GraphQL;
using SwapIndexer.Processors;
using SwapIndexer;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace SwapIndexer.Tests.Processors;


public sealed class LiquidityRecordProcessorTests : SwapIndexerTestBase
{
    private readonly IReadOnlyRepository<LiquidityRecordIndex> _recordRepository;
    private readonly IReadOnlyRepository<UserLiquidityIndex> _userLiquidityRepository;
    private readonly IObjectMapper _objectMapper;
    private readonly LiquidityAddedProcessor _liquidityAddedEventProcessor;
    private readonly LiquidityRemovedProcessor _liquidityRemovedEventProcessor;
    
    const string AddTransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string RemoveTransactionId = "d1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string ChainId = "AELF";

    public LiquidityRecordProcessorTests()
    {
        _recordRepository = GetRequiredService<IReadOnlyRepository<LiquidityRecordIndex>>();
        _userLiquidityRepository = GetRequiredService<IReadOnlyRepository<UserLiquidityIndex>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
        _liquidityAddedEventProcessor = GetRequiredService<LiquidityAddedProcessor>();
        _liquidityRemovedEventProcessor = GetRequiredService<LiquidityRemovedProcessor>();
    }
    
    [Fact]
    public async Task LiquidityGetContractAddressAsyncTests()
    {
        _liquidityAddedEventProcessor.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("2YnkipJ9mty5r6tpTWQAwnomeeKUT7qCWLHKaSeV1fejYEyCdX");
        
        var liquidityAddedEventProcessor2 = GetRequiredService<LiquidityAddedProcessor2>();
        liquidityAddedEventProcessor2.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("fGa81UPViGsVvTM13zuAAwk1QHovL3oSqTrCznitS4hAawPpk");
        
        var liquidityAddedEventProcessor3 = GetRequiredService<LiquidityAddedProcessor3>();
        liquidityAddedEventProcessor3.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("LzkrbEK2zweeuE4P8Y23BMiFY2oiKMWyHuy5hBBbF1pAPD2hh");
        
        var liquidityAddedEventProcessor4 = GetRequiredService<LiquidityAddedProcessor4>();
        liquidityAddedEventProcessor4.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("EG73zzQqC8JencoFEgCtrEUvMBS2zT22xoRse72XkyhuuhyTC");
        
        var liquidityAddedEventProcessor5 = GetRequiredService<LiquidityAddedProcessor5>();
        liquidityAddedEventProcessor5.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("23dh2s1mXnswi4yNW7eWNKWy7iac8KrXJYitECgUctgfwjeZwP");
        
        
        _liquidityRemovedEventProcessor.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("2YnkipJ9mty5r6tpTWQAwnomeeKUT7qCWLHKaSeV1fejYEyCdX");
        
        var liquidityRemovedEventProcessor2 = GetRequiredService<LiquidityRemovedProcessor2>();
        liquidityRemovedEventProcessor2.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("fGa81UPViGsVvTM13zuAAwk1QHovL3oSqTrCznitS4hAawPpk");
        
        var liquidityRemovedEventProcessor3 = GetRequiredService<LiquidityRemovedProcessor3>();
        liquidityRemovedEventProcessor3.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("LzkrbEK2zweeuE4P8Y23BMiFY2oiKMWyHuy5hBBbF1pAPD2hh");
        
        var liquidityRemovedEventProcessor4 = GetRequiredService<LiquidityRemovedProcessor4>();
        liquidityRemovedEventProcessor4.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("EG73zzQqC8JencoFEgCtrEUvMBS2zT22xoRse72XkyhuuhyTC");
        
        var liquidityRemovedEventProcessor5 = GetRequiredService<LiquidityRemovedProcessor5>();
        liquidityRemovedEventProcessor5.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("23dh2s1mXnswi4yNW7eWNKWy7iac8KrXJYitECgUctgfwjeZwP");
    }

    [Fact]
    public async Task LiquidityAddedAsyncTests()
    {
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        
        var liquidityAdd = new LiquidityAdded()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolA = "AELF",
            SymbolB = "BTC",
            AmountA = 100,
            AmountB = 1,
            LiquidityToken = 1,
            Channel = "test"
        };
        var logEventContext = GenerateLogEventContext(liquidityAdd);
        logEventContext.Transaction.TransactionId = transactionId;
        
        await _liquidityAddedEventProcessor.ProcessAsync(liquidityAdd, logEventContext);
        
        var userLiquidityIndexData = await SwapIndexerTestHelper.GetEntityAsync( _userLiquidityRepository, ChainId + "-" + liquidityAdd.To.ToBase58() + "-" + liquidityAdd.Pair.ToBase58());
        userLiquidityIndexData.Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        userLiquidityIndexData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        userLiquidityIndexData.LpTokenAmount.ShouldBe(liquidityAdd.LiquidityToken);
        
        var liquidityRecordData = await SwapIndexerTestHelper.GetEntityAsync( _recordRepository, ChainId + "-" + transactionId);
        liquidityRecordData.TransactionHash.ShouldBe(transactionId);
        liquidityRecordData.Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        liquidityRecordData.Sender.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        liquidityRecordData.To.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        liquidityRecordData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        liquidityRecordData.Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        liquidityRecordData.Token0.ShouldBe("AELF");
        liquidityRecordData.Token1.ShouldBe("BTC");
        liquidityRecordData.Token0Amount.ShouldBe(100);
        liquidityRecordData.Token1Amount.ShouldBe(1);
    }

    [Fact]
    public async Task LiquidityAddedSpecialTokenAsyncTests()
    {
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
       
        var liquidityAdd = new LiquidityAdded()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolA = "SGR-1",
            SymbolB = "ELF",
            AmountA = 100,
            AmountB = 1,
            LiquidityToken = 1,
            Channel = "test"
        };
        var logEventContext = GenerateLogEventContext(liquidityAdd);
        logEventContext.Transaction.TransactionId = transactionId;
        
        await _liquidityAddedEventProcessor.ProcessAsync(liquidityAdd, logEventContext);
        
        var userLiquidityIndexData = await SwapIndexerTestHelper.GetEntityAsync( _userLiquidityRepository, ChainId + "-" + liquidityAdd.To.ToBase58() + "-" + liquidityAdd.Pair.ToBase58());
        userLiquidityIndexData.Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        userLiquidityIndexData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        userLiquidityIndexData.LpTokenAmount.ShouldBe(liquidityAdd.LiquidityToken);
        
        var liquidityRecordData = await SwapIndexerTestHelper.GetEntityAsync( _recordRepository, ChainId + "-" + transactionId);
        liquidityRecordData.TransactionHash.ShouldBe(transactionId);
        liquidityRecordData.Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        liquidityRecordData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        liquidityRecordData.Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        liquidityRecordData.Token0.ShouldBe("SGR-1");
        liquidityRecordData.Token1.ShouldBe("ELF");
        liquidityRecordData.Token0Amount.ShouldBe(100);
        liquidityRecordData.Token1Amount.ShouldBe(1);
    }
    
    [Fact]
    public async Task LiquidityRemovedAsyncTests()
    {
        await LiquidityAddedAsyncTests();
       
        const string transactionId = "d1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        
        var liquidityRemove = new LiquidityRemoved()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolA = "AELF",
            SymbolB = "BTC",
            AmountA = 100,
            AmountB = 1,
            LiquidityToken = 1
        };
        var logEventContext = GenerateLogEventContext(liquidityRemove);
        logEventContext.Transaction.TransactionId = transactionId;

        await _liquidityRemovedEventProcessor.ProcessAsync(liquidityRemove, logEventContext);
        
        var userLiquidityIndexData = await SwapIndexerTestHelper.GetEntityAsync( _userLiquidityRepository, ChainId + "-" + liquidityRemove.To.ToBase58() + "-" + liquidityRemove.Pair.ToBase58());
        userLiquidityIndexData.Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        userLiquidityIndexData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        userLiquidityIndexData.LpTokenAmount.ShouldBe(0);
        
        var liquidityRecordData = await SwapIndexerTestHelper.GetEntityAsync( _recordRepository, ChainId + "-" + transactionId);
        liquidityRecordData.TransactionHash.ShouldBe(transactionId);
        liquidityRecordData.Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        liquidityRecordData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        liquidityRecordData.Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
        liquidityRecordData.Token0.ShouldBe("AELF");
        liquidityRecordData.Token1.ShouldBe("BTC");
        liquidityRecordData.Token0Amount.ShouldBe(100);
        liquidityRecordData.Token1Amount.ShouldBe(1);
    }
    
    [Fact]
    public async Task QueryLiquidityRecordAsyncTests()
    {
        await LiquidityAddedAsyncTests();
        var result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, new GetLiquidityRecordDto
        {
            SkipCount = 0,
            MaxResultCount = 100,
            ChainId = "AELF",
            Address = Address.FromPublicKey("BBB".HexToByteArray()).ToBase58(),
            Pair = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            TimestampMin = DateTime.UtcNow.ToTimestamp().Seconds * 1000 - 60000,
            TimestampMax = DateTime.UtcNow.ToTimestamp().Seconds * 1000,
            Type = LiquidityRecordIndex.LiquidityType.Mint,
            Token0 = "AELF",
            Token1 = "BTC",
            TokenSymbol = "AELF",
            TransactionHash = AddTransactionId
        });
        result.TotalCount.ShouldBe(1);
        result.Data.First().LpTokenAmount.ShouldBe(1);
        result.Data.First().Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        result.Data.First().Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe("AELF");
    
        await LiquidityRemovedAsyncTests();
        var recordDto = new GetLiquidityRecordDto
        {
            SkipCount = 0,
            MaxResultCount = 100,
            ChainId = "AELF",
            Address = Address.FromPublicKey("BBB".HexToByteArray()).ToBase58(),
            Pair = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            TimestampMin = DateTime.UtcNow.ToTimestamp().Seconds * 1000 - 60000,
            TimestampMax = DateTime.UtcNow.ToTimestamp().Seconds * 1000
        };
        result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(2);
        result.Data.First().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
        result.Data.Last().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        
        recordDto.Sorting = "timestamp";
        result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(2);
        result.Data.First().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        result.Data.Last().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
    
        recordDto.Sorting = "timestamp asc";
        result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(2);
        result.Data.First().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        result.Data.Last().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
        
        recordDto.Sorting = "timestamp ascend";
        result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(2);
        result.Data.First().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        result.Data.Last().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
        
        recordDto.Sorting = "timestamp desc";
        result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(2);
        result.Data.First().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
        result.Data.Last().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        
        recordDto.Sorting = "timestamp descend";
        result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(2);
        result.Data.First().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
        result.Data.Last().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        
        recordDto.Sorting = "tradepair";
        result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(2);
        result.Data.First().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        result.Data.Last().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
        
        recordDto.Sorting = "tradepair desc";
        result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(2);
        result.Data.First().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        result.Data.Last().Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Burn);
    }
    
    
    [Fact]
    public async Task QueryLiquidityRecordSpecialTokenAsyncTests()
    {
        await LiquidityAddedSpecialTokenAsyncTests();
        var result = await Query.LiquidityRecordAsync(_recordRepository, _objectMapper, new GetLiquidityRecordDto
        {
            SkipCount = 0,
            MaxResultCount = 100,
            ChainId = "AELF",
            Address = Address.FromPublicKey("BBB".HexToByteArray()).ToBase58(),
            TokenSymbol = "sgr"
        });
        result.TotalCount.ShouldBe(1);
        result.Data.First().LpTokenAmount.ShouldBe(1);
        result.Data.First().Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        result.Data.First().Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe("AELF");
        result.Data.First().Token0.ShouldBe("SGR-1");
        result.Data.First().Token1.ShouldBe("ELF");
    }
    
    [Fact]
    public async Task QueryUserLiquidityAsyncTests()
    {
        await LiquidityAddedAsyncTests();
        var recordDto = new GetUserLiquidityDto()
        {
            ChainId = "AELF",
            Address = Address.FromPublicKey("BBB".HexToByteArray()).ToBase58(),
            Pair = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            SkipCount = 0,
            MaxResultCount = 100
        };
        var result = await Query.UserLiquidityAsync(_userLiquidityRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(1);
        result.Data.First().LpTokenAmount.ShouldBe(1);
        result.Data.First().Address.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        result.Data.First().Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe("AELF");
        
        recordDto.Sorting = "timestamp";
        result = await Query.UserLiquidityAsync(_userLiquidityRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(1);
        
        recordDto.Sorting = "lptokenamount";
        result = await Query.UserLiquidityAsync(_userLiquidityRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(1);
        
        recordDto.Sorting = "timestamp desc";
        result = await Query.UserLiquidityAsync(_userLiquidityRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(1);
        
        recordDto.Sorting = "lptokenamount desc";
        result = await Query.UserLiquidityAsync(_userLiquidityRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(1);
    }
    
    [Fact]
    public async Task GetLiquidityRecordsAsyncTest()
    {
        await LiquidityAddedAsyncTests();
        var result = await Query.GetLiquidityRecordsAsync(_recordRepository, _objectMapper, new GetPullLiquidityRecordDto()
        {
            ChainId = "AELF",
            StartBlockHeight = 100,
            EndBlockHeight = 100
        });
        result.Count.ShouldBe(1);
        result.First().TransactionHash.ShouldBe(AddTransactionId);
        result.First().BlockHeight.ShouldBe(100);
        
        await LiquidityRemovedAsyncTests();
        result = await Query.GetLiquidityRecordsAsync(_recordRepository, _objectMapper, new GetPullLiquidityRecordDto
        {
            ChainId = "AELF",
            StartBlockHeight = 100,
            EndBlockHeight = 100
        });
        result.Count.ShouldBe(2);
        result[1].TransactionHash.ShouldBe(RemoveTransactionId);
        result[1].BlockHeight.ShouldBe(100);
        
        result = await Query.GetLiquidityRecordsAsync(_recordRepository, _objectMapper, new GetPullLiquidityRecordDto
        {
            ChainId = "AELF",
            StartBlockHeight = 100,
            EndBlockHeight = 100,
            SkipCount = 1
        });
        result.Count.ShouldBe(1);
        result.First().TransactionHash.ShouldBe(RemoveTransactionId);
        result.First().BlockHeight.ShouldBe(100);
        
        result = await Query.GetLiquidityRecordsAsync(_recordRepository, _objectMapper, new GetPullLiquidityRecordDto
        {
            ChainId = "AELF",
            StartBlockHeight = 100,
            EndBlockHeight = 100,
            SkipCount = 2
        });
        result.Count.ShouldBe(0);
        
        result = await Query.GetLiquidityRecordsAsync(_recordRepository, _objectMapper, new GetPullLiquidityRecordDto
        {
            ChainId = "AELF",
            StartBlockHeight = 100,
            EndBlockHeight = 100,
            MaxResultCount = 1
        });
        result.Count.ShouldBe(1);
        result.First().TransactionHash.ShouldBe(AddTransactionId);
        result.First().BlockHeight.ShouldBe(100);
        
        Func<Task> action = async () => await Query.GetLiquidityRecordsAsync(_recordRepository, _objectMapper, new GetPullLiquidityRecordDto
        {
            ChainId = "AELF",
            StartBlockHeight = 100,
            EndBlockHeight = 100,
            MaxResultCount = 10001
        });
        
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
        Assert.Contains("Max allowed value", exception.Message);
    }
}