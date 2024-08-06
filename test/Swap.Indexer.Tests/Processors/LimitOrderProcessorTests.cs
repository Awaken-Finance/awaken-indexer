using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Hooks;
using Awaken.Contracts.Order;
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
public sealed class LimitOrderProcessorTests : SwapIndexerTests
{
    private readonly IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> _recordRepository;
    private readonly IObjectMapper _objectMapper;

    public LimitOrderProcessorTests()
    {
        _recordRepository = GetRequiredService<IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }
    
    const string CreatedTransactionId = "e1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string FilledTransactionId1 = "f1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string CancelledTransactionId = "g1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string RemovedTransactionId = "h1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string FilledTransactionId2 = "i1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";

    [Fact]
    public async Task LimitOrderCreatedAsyncTests()
    {
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = CreatedTransactionId;
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
        DateTime now = DateTime.UtcNow;
        var commitTime = Timestamp.FromDateTime(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc));
        var deadLine = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(1));
        var orderId = 1;
        var limitOrderCreated = new LimitOrderCreated()
        {
            OrderId = orderId,
            AmountIn = 1000,
            AmountOut = 0,
            CommitTime = commitTime,
            Deadline = deadLine,
            Maker = Address.FromPublicKey("AAA".HexToByteArray()),
            SymbolIn = "ELF",
            SymbolOut = "BTC"
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(limitOrderCreated.ToLogEvent());
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
            Params = "{}",
            MethodName = "CommitLimitOrder",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var limitOrderCreatedEventProcessor = GetRequiredService<LimitOrderCreatedProcessor>();
        await limitOrderCreatedEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        limitOrderCreatedEventProcessor.GetContractAddress(chainId).ShouldBe("CCCCCC");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var limitOrderIndexData = await _recordRepository.GetAsync(IdGenerateHelper.GetId(orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(0);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.Deadline.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(deadLine.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.Committed);
        limitOrderIndexData.TransactionHash.ShouldBe(CreatedTransactionId);
    }
    
    
    [Fact]
    public async Task LimitOrderFilled1AsyncTests()
    {
        await LimitOrderCreatedAsyncTests();
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = FilledTransactionId1;
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
        DateTime now = DateTime.UtcNow;
        var commitDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var commitTime = Timestamp.FromDateTime(commitDay);
        var fillTime = Timestamp.FromDateTime(commitDay.AddHours(1));
        var orderId = 1;
        var limitOrderCreated = new LimitOrderFilled()
        {
            OrderId = orderId,
            FillTime = fillTime,
            AmountInFilled = 100,
            Taker = Address.FromPublicKey("BBB".HexToByteArray())
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(limitOrderCreated.ToLogEvent());
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
            Params = "{}",
            MethodName = "FillLimitOrder",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var processor = GetRequiredService<LimitOrderFilledProcessor>();
        await processor.HandleEventAsync(logEventInfo, logEventContext);
        processor.GetContractAddress(chainId).ShouldBe("CCCCCC");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var limitOrderIndexData = await _recordRepository.GetAsync(IdGenerateHelper.GetId(orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(0);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.FillTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(fillTime.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.PartiallyFilling);
        limitOrderIndexData.AmountInFilled.ShouldBe(100);
        limitOrderIndexData.FillRecords.Count.ShouldBe(1);
        limitOrderIndexData.FillRecords[0].AmountInFilled.ShouldBe(100);
        limitOrderIndexData.FillRecords[0].TakerAddress.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        limitOrderIndexData.FillRecords[0].TransactionTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(fillTime.ToDateTime()));
        limitOrderIndexData.FillRecords[0].TransactionHash.ShouldBe(FilledTransactionId1);
    }
    
    [Fact]
    public async Task LimitOrderFilled2AsyncTests()
    {
        await LimitOrderFilled1AsyncTests();
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = FilledTransactionId2;
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
        DateTime now = DateTime.UtcNow;
        var commitDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var commitTime = Timestamp.FromDateTime(commitDay);
        var fillTime1 = Timestamp.FromDateTime(commitDay.AddHours(1));
        var fillTime2 = Timestamp.FromDateTime(commitDay.AddHours(2));
        var orderId = 1;
        var limitOrderCreated = new LimitOrderFilled()
        {
            OrderId = orderId,
            FillTime = fillTime2,
            AmountInFilled = 900,
            Taker = Address.FromPublicKey("CCC".HexToByteArray())
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(limitOrderCreated.ToLogEvent());
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
            Params = "{}",
            MethodName = "FillLimitOrder",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var processor = GetRequiredService<LimitOrderFilledProcessor>();
        await processor.HandleEventAsync(logEventInfo, logEventContext);
        processor.GetContractAddress(chainId).ShouldBe("CCCCCC");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var limitOrderIndexData = await _recordRepository.GetAsync(IdGenerateHelper.GetId(orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(0);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.FillTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(fillTime2.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.FullFilled);
        limitOrderIndexData.AmountInFilled.ShouldBe(1000);
        limitOrderIndexData.FillRecords.Count.ShouldBe(2);
        limitOrderIndexData.FillRecords[0].AmountInFilled.ShouldBe(100);
        limitOrderIndexData.FillRecords[0].TakerAddress.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        limitOrderIndexData.FillRecords[0].TransactionTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(fillTime1.ToDateTime()));
        limitOrderIndexData.FillRecords[0].TransactionHash.ShouldBe(FilledTransactionId1);
        limitOrderIndexData.FillRecords[0].Status.ShouldBe(LimitOrderRecordStatus.Fill);
        limitOrderIndexData.FillRecords[1].AmountInFilled.ShouldBe(900);
        limitOrderIndexData.FillRecords[1].TakerAddress.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        limitOrderIndexData.FillRecords[1].TransactionTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(fillTime2.ToDateTime()));
        limitOrderIndexData.FillRecords[1].TransactionHash.ShouldBe(FilledTransactionId2);
        limitOrderIndexData.FillRecords[1].Status.ShouldBe(LimitOrderRecordStatus.Fill);
    }
    
    [Fact]
    public async Task LimitOrderCancelledAsyncTests()
    {
        await LimitOrderFilled1AsyncTests();
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = CancelledTransactionId;
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
        DateTime now = DateTime.UtcNow;
        var commitDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var commitTime = Timestamp.FromDateTime(commitDay);
        var fillTime1 = Timestamp.FromDateTime(commitDay.AddHours(1));
        var cancelledTime = Timestamp.FromDateTime(commitDay.AddHours(2));
        var orderId = 1;
        var limitOrderCancelled = new LimitOrderCancelled()
        {
            OrderId = orderId,
            CancelTime = cancelledTime
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(limitOrderCancelled.ToLogEvent());
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
            Params = "{}",
            MethodName = "CancelLimitOrder",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var processor = GetRequiredService<LimitOrderCancelledProcessor>();
        await processor.HandleEventAsync(logEventInfo, logEventContext);
        processor.GetContractAddress(chainId).ShouldBe("CCCCCC");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var limitOrderIndexData = await _recordRepository.GetAsync(IdGenerateHelper.GetId(orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(0);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.CancelTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(cancelledTime.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.Cancelled);
        limitOrderIndexData.AmountInFilled.ShouldBe(100);
        limitOrderIndexData.FillRecords.Count.ShouldBe(2);
        limitOrderIndexData.FillRecords[1].TransactionTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(cancelledTime.ToDateTime()));
        limitOrderIndexData.FillRecords[1].TransactionHash.ShouldBe(CancelledTransactionId);
        limitOrderIndexData.FillRecords[1].Status.ShouldBe(LimitOrderRecordStatus.Cancel);
        
        var result = await Query.LimitOrderAsync(_recordRepository, _objectMapper, new GetLimitOrderDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            LimitOrderStatus = LimitOrderStatus.Cancelled
        });
        result.Data.Count.ShouldBe(1);
        result.Data[0].OrderId.ShouldBe(1);
    }
    
    
    [Fact]
    public async Task LimitOrderRemovedAsyncTests()
    {
        await LimitOrderFilled1AsyncTests();
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = RemovedTransactionId;
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
        DateTime now = DateTime.UtcNow;
        var commitDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var commitTime = Timestamp.FromDateTime(commitDay);
        var removedTime = Timestamp.FromDateTime(commitDay.AddHours(2));
        var orderId = 1;
        var limitOrderCancelled = new LimitOrderRemoved()
        {
            OrderId = orderId,
            RemoveTime = removedTime
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(limitOrderCancelled.ToLogEvent());
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
            Params = "{}",
            MethodName = "FillLimitOrder",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var processor = GetRequiredService<LimitOrderRemovedProcessor>();
        await processor.HandleEventAsync(logEventInfo, logEventContext);
        processor.GetContractAddress(chainId).ShouldBe("CCCCCC");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var limitOrderIndexData = await _recordRepository.GetAsync(IdGenerateHelper.GetId(orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(0);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.RemoveTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(removedTime.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.Removed);
        limitOrderIndexData.AmountInFilled.ShouldBe(100);
        limitOrderIndexData.FillRecords.Count.ShouldBe(1);
        
        var result = await Query.LimitOrderAsync(_recordRepository, _objectMapper, new GetLimitOrderDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            LimitOrderStatus = LimitOrderStatus.Removed
        });
        result.Data.Count.ShouldBe(1);
        result.Data[0].OrderId.ShouldBe(1);
    }

    [Fact]
    public async Task QueryLimitOrderAsyncTests()
    {
        await LimitOrderFilled2AsyncTests();
        
        DateTime now = DateTime.UtcNow;
        var commitDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var commitTime = Timestamp.FromDateTime(commitDay);
        
        // by maker address
        var result = await Query.LimitOrderAsync(_recordRepository, _objectMapper, new GetLimitOrderDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            MakerAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58()
        });
        
        result.TotalCount.ShouldBe(1);
        result.Data[0].Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data[0].OrderId.ShouldBe(1);
        result.Data[0].AmountIn.ShouldBe(1000);
        result.Data[0].AmountOut.ShouldBe(0);
        result.Data[0].CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        result.Data[0].SymbolIn.ShouldBe("ELF");
        result.Data[0].SymbolOut.ShouldBe("BTC");
        result.Data[0].LimitOrderStatus.ShouldBe(LimitOrderStatus.FullFilled);
        result.Data[0].AmountInFilled.ShouldBe(1000);
        result.Data[0].FillRecords.Count.ShouldBe(2);
        
        // by maker address and status
        result = await Query.LimitOrderAsync(_recordRepository, _objectMapper, new GetLimitOrderDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            MakerAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            LimitOrderStatus = LimitOrderStatus.FullFilled
        });
        result.Data.Count.ShouldBe(1);
        result.Data[0].OrderId.ShouldBe(1);
        
        // by order id
        result = await Query.LimitOrderAsync(_recordRepository, _objectMapper, new GetLimitOrderDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            OrderId = 1
        });
        result.Data.Count.ShouldBe(1);
        result.Data[0].OrderId.ShouldBe(1);

    }
}