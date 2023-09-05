using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Swap;
using Google.Protobuf.WellKnownTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Orleans;
using Shouldly;
using Swap.Indexer.Entities;
using Swap.Indexer.GraphQL;
using Swap.Indexer.Orleans.TestBase;
using Swap.Indexer.Processors;
using Swap.Indexer.Tests.Helper;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Swap.Indexer.Tests.Processors;

[Collection(ClusterCollection.Name)]
public sealed class LiquidityRecordProcessorTests : SwapIndexerTests
{
    private readonly IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> _recordRepository;
    private readonly IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> _userLiquidityRepository;
    private readonly IObjectMapper _objectMapper;

    public LiquidityRecordProcessorTests()
    {
        _recordRepository = GetRequiredService<IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo>>();
        _userLiquidityRepository = GetRequiredService<IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }
    
    const string AddTransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string RemoveTransactionId = "d1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";

    [Fact]
    public async Task LiquidityAddedAsyncTests()
    {
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);
        
        // step2: create logEventInfo
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
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(liquidityAdd.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId= chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            Params = "{ \"to\": \"swap\", \"symbol\": \"ELF\", \"amount\": \"100000000000\" }",
            MethodName = "LiquidityAdd",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var liquidityAddedEventProcessor = GetRequiredService<LiquidityAddedProcessor>();
        await liquidityAddedEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        liquidityAddedEventProcessor.GetContractAddress(chainId).ShouldBe("XXXXXX");
        
        var liquidityAddedEventProcessor2 = GetRequiredService<LiquidityAddedProcessor2>();
        liquidityAddedEventProcessor2.GetContractAddress(chainId).ShouldBe("XXXXXX2");
        
        var liquidityAddedEventProcessor3 = GetRequiredService<LiquidityAddedProcessor3>();
        liquidityAddedEventProcessor3.GetContractAddress(chainId).ShouldBe("XXXXXX3");
        
        var liquidityAddedEventProcessor4 = GetRequiredService<LiquidityAddedProcessor4>();
        liquidityAddedEventProcessor4.GetContractAddress(chainId).ShouldBe("XXXXXX4");
        
        var liquidityAddedEventProcessor5 = GetRequiredService<LiquidityAddedProcessor5>();
        liquidityAddedEventProcessor5.GetContractAddress(chainId).ShouldBe("XXXXXX5");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var userLiquidityIndexData = await _userLiquidityRepository.GetAsync(chainId + "-" + liquidityAdd.Sender.ToBase58() + "-" + liquidityAdd.Pair.ToBase58());
        userLiquidityIndexData.Address.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        userLiquidityIndexData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        userLiquidityIndexData.LpTokenAmount.ShouldBe(liquidityAdd.LiquidityToken);
        
        var liquidityRecordData = await _recordRepository.GetAsync(chainId + "-" + transactionId);
        liquidityRecordData.TransactionHash.ShouldBe(transactionId);
        liquidityRecordData.Address.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        liquidityRecordData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        liquidityRecordData.Type.ShouldBe(LiquidityRecordIndex.LiquidityType.Mint);
        liquidityRecordData.Token0.ShouldBe("AELF");
        liquidityRecordData.Token1.ShouldBe("BTC");
        liquidityRecordData.Token0Amount.ShouldBe(100);
        liquidityRecordData.Token1Amount.ShouldBe(1);
    }
    
    [Fact]
    public async Task LiquidityRemovedAsyncTests()
    {
        await LiquidityAddedAsyncTests();
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "d1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        const long blockHeight = 100;
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
        {
            BlockHash = blockHash,
            BlockHeight = blockHeight,
            Confirmed = true,
            PreviousBlockHash = previousBlockHash,
        };
        
        var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
        var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);
        
        // step2: create logEventInfo
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
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(liquidityRemove.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId= chainId;
        logEventInfo.BlockHash = blockHash;
        logEventInfo.TransactionId = transactionId;
        var logEventContext = new LogEventContext
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            Params = "{ \"to\": \"swap\", \"symbol\": \"ELF\", \"amount\": \"100000000000\" }",
            MethodName = "LiquidityRemoved",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var liquidityRemovedEventProcessor = GetRequiredService<LiquidityRemovedProcessor>();
        await liquidityRemovedEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        liquidityRemovedEventProcessor.GetContractAddress(chainId).ShouldBe("XXXXXX");
        
        var liquidityRemovedEventProcessor2 = GetRequiredService<LiquidityRemovedProcessor2>();
        liquidityRemovedEventProcessor2.GetContractAddress(chainId).ShouldBe("XXXXXX2");
        
        var liquidityRemovedEventProcessor3 = GetRequiredService<LiquidityRemovedProcessor3>();
        liquidityRemovedEventProcessor3.GetContractAddress(chainId).ShouldBe("XXXXXX3");
        
        var liquidityRemovedEventProcessor4 = GetRequiredService<LiquidityRemovedProcessor4>();
        liquidityRemovedEventProcessor4.GetContractAddress(chainId).ShouldBe("XXXXXX4");;
        
        var liquidityRemovedEventProcessor5 = GetRequiredService<LiquidityRemovedProcessor5>();
        liquidityRemovedEventProcessor5.GetContractAddress(chainId).ShouldBe("XXXXXX5");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var userLiquidityIndexData = await _userLiquidityRepository.GetAsync(chainId + "-" + liquidityRemove.Sender.ToBase58() + "-" + liquidityRemove.Pair.ToBase58());
        userLiquidityIndexData.Address.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        userLiquidityIndexData.Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        userLiquidityIndexData.LpTokenAmount.ShouldBe(0);
        
        var liquidityRecordData = await _recordRepository.GetAsync(chainId + "-" + transactionId);
        liquidityRecordData.TransactionHash.ShouldBe(transactionId);
        liquidityRecordData.Address.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
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
            Address = Address.FromPublicKey("CCC".HexToByteArray()).ToBase58(),
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
        result.Data.First().Address.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        result.Data.First().Pair.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe("AELF");

        await LiquidityRemovedAsyncTests();
        var recordDto = new GetLiquidityRecordDto
        {
            SkipCount = 0,
            MaxResultCount = 100,
            ChainId = "AELF",
            Address = Address.FromPublicKey("CCC".HexToByteArray()).ToBase58(),
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
    public async Task QueryUserLiquidityAsyncTests()
    {
        await LiquidityAddedAsyncTests();
        var recordDto = new GetUserLiquidityDto()
        {
            ChainId = "AELF",
            Address = Address.FromPublicKey("CCC".HexToByteArray()).ToBase58(),
            Pair = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            SkipCount = 0,
            MaxResultCount = 100
        };
        var result = await Query.UserLiquidityAsync(_userLiquidityRepository, _objectMapper, recordDto);
        result.TotalCount.ShouldBe(1);
        result.Data.First().LpTokenAmount.ShouldBe(1);
        result.Data.First().Address.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
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
    }

    [Fact]
    public async Task SyncStateAsyncTests()
    {
        await LiquidityAddedAsyncTests();
        var aelfIndexerClientInfoProvider = GetRequiredService<IAElfIndexerClientInfoProvider>();
        var clusterClient = GetRequiredService<IClusterClient>();
        
        var result = await Query.SyncStateAsync(clusterClient, aelfIndexerClientInfoProvider, new GetSyncStateDto
        {
            ChainId = "AELF",
            FilterType = BlockFilterType.LogEvent
        });
        await Task.Delay(1000);
        // unit cann't update confirmHeight
        result.ConfirmedBlockHeight.ShouldBeGreaterThanOrEqualTo(0);
    }
}