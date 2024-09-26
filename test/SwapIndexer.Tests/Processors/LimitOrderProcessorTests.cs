using AeFinder.Sdk;
using AeFinder.Sdk.Logging;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using Awaken.Contracts.Hooks;
using Awaken.Contracts.Order;
using Awaken.Contracts.Swap;
using Google.Protobuf;
using SwapIndexer.Entities;
using SwapIndexer.GraphQL;
using SwapIndexer.Processors;
using SwapIndexer.Providers;
using Google.Protobuf.WellKnownTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
using Swap.Indexer.Processors;
using Volo.Abp.ObjectMapping;
using Xunit;
using LogEvent = AeFinder.Sdk.Processor.LogEvent;

namespace SwapIndexer.Tests.Processors;


public sealed class LimitOrderProcessorTests : SwapIndexerTestBase
{
    private readonly IReadOnlyRepository<LimitOrderIndex> _recordRepository;
    private readonly IReadOnlyRepository<SwapRecordIndex> _swapRecordRepository;
    private readonly IReadOnlyRepository<LiquidityRecordIndex> _liquidityRepository;
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfDataProvider _aelfDataProvider;
    private readonly IAeFinderLogger _logger;
    private readonly LimitOrderCreatedProcessor _limitOrderCreatedProcessor;
    private readonly LimitOrderFilledProcessor _limitOrderFilledProcessor;
    private readonly LimitOrderCancelledProcessor _limitOrderCancelledProcessor;
    private readonly LimitOrderRemovedProcessor _limitOrderRemovedProcessor;
    
    const string CreatedTransactionId = "e1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string FilledTransactionId1 = "f1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string CancelledTransactionId = "g1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string RemovedTransactionId = "h1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string FilledTransactionId2 = "i1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    const string ChainId = "AELF";

    public readonly DateTime CommitDay;
    public readonly Timestamp CommitTime;
    public readonly Timestamp FillTime;
    public LimitOrderProcessorTests()
    {
        _recordRepository = GetRequiredService<IReadOnlyRepository<LimitOrderIndex>>();
        _swapRecordRepository = GetRequiredService<IReadOnlyRepository<SwapRecordIndex>>();
        _liquidityRepository = GetRequiredService<IReadOnlyRepository<LiquidityRecordIndex>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
        _aelfDataProvider = GetRequiredService<IAElfDataProvider>();
        _logger = GetRequiredService<IAeFinderLogger>();
        _limitOrderCreatedProcessor = GetRequiredService<LimitOrderCreatedProcessor>();
        _limitOrderFilledProcessor = GetRequiredService<LimitOrderFilledProcessor>();
        _limitOrderCancelledProcessor = GetRequiredService<LimitOrderCancelledProcessor>();
        _limitOrderRemovedProcessor = GetRequiredService<LimitOrderRemovedProcessor>();
        DateTime now = DateTime.UtcNow;
        CommitDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        CommitTime = Timestamp.FromDateTime(CommitDay);
        FillTime = Timestamp.FromDateTime(CommitDay.AddHours(1));
    }
    
    

    [Fact]
    public async Task LimitOrderCreatedAsyncTests()
    {
        DateTime now = DateTime.UtcNow;
        var commitTime = Timestamp.FromDateTime(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc));
        var deadLine = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(1));
        var orderId = 1;
        var limitOrderCreated = new LimitOrderCreated()
        {
            OrderId = orderId,
            AmountIn = 1000,
            AmountOut = 100,
            CommitTime = commitTime,
            Deadline = deadLine,
            Maker = Address.FromPublicKey("AAA".HexToByteArray()),
            SymbolIn = "ELF",
            SymbolOut = "BTC"
        };
        var logEventContext = GenerateLogEventContext(limitOrderCreated);
        logEventContext.Transaction.TransactionId = CreatedTransactionId;
        
        await _limitOrderCreatedProcessor.ProcessAsync(limitOrderCreated, logEventContext);
        
        var limitOrderIndexData =
            await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, IdGenerateHelper.GetId(ChainId, orderId));
        
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Metadata.ChainId.ShouldBe(ChainId);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(100);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.Deadline.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(deadLine.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LastUpdateTime.ShouldBe(limitOrderIndexData.CommitTime);
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.Committed);
        limitOrderIndexData.TransactionHash.ShouldBe(logEventContext.Transaction.TransactionId);
        limitOrderIndexData.TransactionFee.ShouldBe(0);
        limitOrderIndexData.Metadata.ChainId.ShouldBe(ChainId);
        limitOrderIndexData.Metadata.Block.BlockHeight.ShouldBe(logEventContext.Block.BlockHeight);
        limitOrderIndexData.Metadata.Block.BlockHash.ShouldBe(logEventContext.Block.BlockHash);

        var limitOrders =
            await Query.GetLimitOrdersAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto()
            {
                StartBlockHeight = 0,
                EndBlockHeight = 100,
                SkipCount = 0,
                MaxResultCount = 100
            });
        limitOrders.Count.ShouldBe(1);
        limitOrders[0].OrderId.ShouldBe(1);
    }
    
    
    [Fact]
    public async Task LimitOrderFilled1AsyncTests()
    {
        await LimitOrderCreatedAsyncTests();
        
        var orderId = 1;
        var limitOrderFilled = new LimitOrderFilled()
        {
            OrderId = orderId,
            FillTime = FillTime,
            AmountInFilled = 100,
            AmountOutFilled = 10,
            Taker = Address.FromPublicKey("BBB".HexToByteArray()),
            TotalFee = 1234
        };
        var logEventContext = GenerateLogEventContext(limitOrderFilled);
        logEventContext.Transaction.TransactionId = FilledTransactionId1;
        
        await _limitOrderFilledProcessor.ProcessAsync(limitOrderFilled, logEventContext);
       
        var limitOrderIndexData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, IdGenerateHelper.GetId(ChainId, orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(100);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(CommitTime.ToDateTime()));
        limitOrderIndexData.FillTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(FillTime.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.PartiallyFilling);
        limitOrderIndexData.AmountInFilled.ShouldBe(100);
        limitOrderIndexData.AmountOutFilled.ShouldBe(10);
        limitOrderIndexData.FillRecords.Count.ShouldBe(1);
        limitOrderIndexData.FillRecords[0].AmountInFilled.ShouldBe(100);
        limitOrderIndexData.FillRecords[0].AmountOutFilled.ShouldBe(10);
        limitOrderIndexData.FillRecords[0].TakerAddress.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        limitOrderIndexData.FillRecords[0].TransactionTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(FillTime.ToDateTime()));
        limitOrderIndexData.FillRecords[0].TransactionHash.ShouldBe(FilledTransactionId1);
        limitOrderIndexData.FillRecords[0].TransactionFee.ShouldBe(0);
        
        var limitOrders =
            await Query.GetLimitOrdersAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto()
            {
                StartBlockHeight = 0,
                EndBlockHeight = 100,
                SkipCount = 0,
                MaxResultCount = 100
            });
        limitOrders.Count.ShouldBe(1);
        limitOrders[0].OrderId.ShouldBe(1);
    }
    
    [Fact]
    public async Task LimitOrderFilled2AsyncTests()
    {
        await LimitOrderFilled1AsyncTests();
        
        DateTime now = DateTime.UtcNow;
        var commitDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var commitTime = Timestamp.FromDateTime(commitDay);
        var fillTime1 = Timestamp.FromDateTime(commitDay.AddHours(1));
        var fillTime2 = Timestamp.FromDateTime(commitDay.AddHours(2));
        var orderId = 1;
        var limitOrderFilled = new LimitOrderFilled()
        {
            OrderId = orderId,
            FillTime = fillTime2,
            AmountInFilled = 900,
            AmountOutFilled = 90,
            Taker = Address.FromPublicKey("AAA".HexToByteArray())
        };
        var logEventContext = GenerateLogEventContext(limitOrderFilled);
        logEventContext.Transaction.TransactionId = FilledTransactionId2;

        await _limitOrderFilledProcessor.ProcessAsync(limitOrderFilled, logEventContext);
        
        //step5: check result
        var limitOrderIndexData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, IdGenerateHelper.GetId(ChainId, orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(100);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.FillTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(fillTime2.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.FullFilled);
        limitOrderIndexData.AmountInFilled.ShouldBe(1000);
        limitOrderIndexData.FillRecords.Count.ShouldBe(2);
        limitOrderIndexData.FillRecords[0].AmountInFilled.ShouldBe(100);
        limitOrderIndexData.FillRecords[0].AmountOutFilled.ShouldBe(10);
        limitOrderIndexData.FillRecords[0].TakerAddress.ShouldBe(Address.FromPublicKey("BBB".HexToByteArray()).ToBase58());
        limitOrderIndexData.FillRecords[0].TransactionTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(fillTime1.ToDateTime()));
        limitOrderIndexData.FillRecords[0].TransactionHash.ShouldBe(FilledTransactionId1);
        limitOrderIndexData.FillRecords[0].Status.ShouldBe(LimitOrderStatus.PartiallyFilling);
        limitOrderIndexData.FillRecords[1].AmountInFilled.ShouldBe(900);
        limitOrderIndexData.FillRecords[1].AmountOutFilled.ShouldBe(90);
        limitOrderIndexData.FillRecords[1].TakerAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.FillRecords[1].TransactionTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(fillTime2.ToDateTime()));
        limitOrderIndexData.FillRecords[1].TransactionHash.ShouldBe(FilledTransactionId2);
        limitOrderIndexData.FillRecords[1].TransactionFee.ShouldBe(0);
        limitOrderIndexData.FillRecords[1].Status.ShouldBe(LimitOrderStatus.PartiallyFilling);
    }
    
    [Fact]
    public async Task LimitOrderCancelledAsyncTests()
    {
        await LimitOrderFilled1AsyncTests();
       
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
        var logEventContext = GenerateLogEventContext(limitOrderCancelled);
        logEventContext.Transaction.TransactionId = CancelledTransactionId;
        
        await _limitOrderCancelledProcessor.ProcessAsync(limitOrderCancelled, logEventContext);
       
        var limitOrderIndexData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, IdGenerateHelper.GetId(ChainId, orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(100);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.CancelTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(cancelledTime.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.Cancelled);
        limitOrderIndexData.AmountInFilled.ShouldBe(100);
        limitOrderIndexData.AmountOutFilled.ShouldBe(10);
        limitOrderIndexData.FillRecords.Count.ShouldBe(2);
        limitOrderIndexData.FillRecords[1].TransactionTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(cancelledTime.ToDateTime()));
        limitOrderIndexData.FillRecords[1].TransactionHash.ShouldBe(CancelledTransactionId);
        limitOrderIndexData.FillRecords[1].TransactionFee.ShouldBe(0);
        limitOrderIndexData.FillRecords[1].Status.ShouldBe(LimitOrderStatus.Cancelled);
        
        var result = await Query.LimitOrderAsync(_recordRepository, _objectMapper, new GetLimitOrderDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            LimitOrderStatus = (int)LimitOrderStatus.Cancelled
        });
        result.Data.Count.ShouldBe(1);
        result.Data[0].OrderId.ShouldBe(1);
        result.Data[0].ChainId.ShouldBe(ChainId);
    }
    
    
    [Fact]
    public async Task LimitOrderRemovedAsyncTests()
    {
        await LimitOrderFilled1AsyncTests();
       
        DateTime now = DateTime.UtcNow;
        var commitDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var commitTime = Timestamp.FromDateTime(commitDay);
        var removedTime = Timestamp.FromDateTime(commitDay.AddHours(2));
        var orderId = 1;
        var limitOrderRemoved = new LimitOrderRemoved()
        {
            OrderId = orderId,
            RemoveTime = removedTime,
            ReasonType = ReasonType.Expired
        };
        var logEventContext = GenerateLogEventContext(limitOrderRemoved);
        logEventContext.Transaction.TransactionId = RemovedTransactionId;
        
        await _limitOrderRemovedProcessor.ProcessAsync(limitOrderRemoved, logEventContext);
        
        var limitOrderIndexData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, IdGenerateHelper.GetId(ChainId, orderId));
        limitOrderIndexData.ShouldNotBe(null);
        limitOrderIndexData.Maker.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        limitOrderIndexData.OrderId.ShouldBe(1);
        limitOrderIndexData.AmountIn.ShouldBe(1000);
        limitOrderIndexData.AmountOut.ShouldBe(100);
        limitOrderIndexData.CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        limitOrderIndexData.RemoveTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(removedTime.ToDateTime()));
        limitOrderIndexData.SymbolIn.ShouldBe("ELF");
        limitOrderIndexData.SymbolOut.ShouldBe("BTC");
        limitOrderIndexData.LimitOrderStatus.ShouldBe(LimitOrderStatus.Expired);
        limitOrderIndexData.AmountInFilled.ShouldBe(100);
        limitOrderIndexData.AmountOutFilled.ShouldBe(10);
        limitOrderIndexData.FillRecords.Count.ShouldBe(2);
        
        var result = await Query.LimitOrderAsync(_recordRepository, _objectMapper, new GetLimitOrderDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            LimitOrderStatus = (int)LimitOrderStatus.Expired
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
        result.Data[0].AmountOut.ShouldBe(100);
        result.Data[0].CommitTime.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(commitTime.ToDateTime()));
        result.Data[0].SymbolIn.ShouldBe("ELF");
        result.Data[0].SymbolOut.ShouldBe("BTC");
        result.Data[0].LimitOrderStatus.ShouldBe(LimitOrderStatus.FullFilled);
        result.Data[0].AmountInFilled.ShouldBe(1000);
        result.Data[0].AmountOutFilled.ShouldBe(100);
        result.Data[0].FillRecords.Count.ShouldBe(2);
        result.Data[0].FillRecords[0].TotalFee.ShouldBe(1234);
        
        // by maker address and status
        result = await Query.LimitOrderAsync(_recordRepository, _objectMapper, new GetLimitOrderDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            MakerAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            LimitOrderStatus = (int)LimitOrderStatus.FullFilled
        });
        result.Data.Count.ShouldBe(1);
        result.Data[0].OrderId.ShouldBe(1);
        
        // by order id
        result = await Query.LimitOrderDetailAsync(_recordRepository, _objectMapper, new GetLimitOrderDetailDto()
        {
            OrderId = 1
        });
        result.Data.Count.ShouldBe(1);
        result.Data[0].OrderId.ShouldBe(1);
    }
    
    [Fact]
    public async Task LimitOrderRemainingUnfilledAsyncTests1()
    {
        await LimitOrderFilled1AsyncTests();
        
        var result = await Query.LimitOrderRemainingUnfilledAsync(_recordRepository, _objectMapper, _logger, new GetLimitOrderRemainingUnfilledDto()
        {
            MakerAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            TokenSymbol = "ELF",
            ChainId = "AELF"
        });
        
        result.Value.ShouldBe("900");
        result.OrderCount.ShouldBe(1);
    }
    
    [Fact]
    public async Task LimitOrderRemainingUnfilledAsyncTests2()
    {
        await LimitOrderFilled2AsyncTests();
        
        var result = await Query.LimitOrderRemainingUnfilledAsync(_recordRepository, _objectMapper, _logger, new GetLimitOrderRemainingUnfilledDto()
        {
            MakerAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            TokenSymbol = "ELF",
            ChainId = "AELF"
        });
        
        result.Value.ShouldBe("0");
        result.OrderCount.ShouldBe(0);
    }
    
    
    [Fact]
    public async Task TransactionFeeTests()
    {
        var feeCharged = new TransactionFeeCharged()
        {
            Symbol = "ELF",
            Amount = 123456
        };
        
        var logEventContext = GenerateLogEventContext(feeCharged);
        logEventContext.LogEvent.EventName = nameof(TransactionFeeCharged);
        var transaction = new AeFinder.Sdk.Processor.Transaction()
        {
            LogEvents = new List<LogEvent>()
            {
                logEventContext.LogEvent
            }
        };
        var txnFee = await _limitOrderCancelledProcessor.GetTransactionFeeAsync(transaction);
        txnFee.ShouldBe(123456);
        
        
        var transactionFeeCharged = TransactionFeeCharged.Parser.ParseFrom(
            ByteString.FromBase64("CgNFTEYQoO8P"));
        transactionFeeCharged.Amount.ShouldBe(260000);
    }
    
    
    [Fact]
    public async Task GetLabsFeeAsyncTests()
    {
        await LimitOrderFilled1AsyncTests();
        
        var swap = new Awaken.Contracts.Swap.Swap()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolIn = "BTC",
            SymbolOut = "AELF",
            AmountIn = 100,
            AmountOut = 1,
            TotalFee = 15,
            Channel = "test"
        };
        var labsFeeCharged = new LabsFeeCharged()
        {
            Amount = 123456,
            Symbol = "AELF"
        };
        
        var logEventContext = GenerateLogEventContext(swap);
        logEventContext.Block.BlockTime = FillTime.ToDateTime();

        var swapProcessor = GetRequiredService<SwapProcessor>();
        await swapProcessor.ProcessAsync(swap, logEventContext);
        
        var labsFeeProcessor = GetRequiredService<LabsFeeChargedProcessor>();
        await labsFeeProcessor.ProcessAsync(labsFeeCharged, logEventContext);

        var labsFee = await Query.LabsFeeAsync(_recordRepository, _swapRecordRepository, _objectMapper, new GetTimeRangeDto()
        {
            TimestampMin = FillTime.AddMinutes(-1).ToDateTime().ToUnixTimeMilliseconds(),
            TimestampMax = FillTime.AddMinutes(1).ToDateTime().ToUnixTimeMilliseconds()
        });
        labsFee.Tokens.Count.ShouldBe(2);
        labsFee.Tokens[0].TokenSymbol.ShouldBe("BTC");
        labsFee.Tokens[0].LabsFee.ShouldBe(1234);
        labsFee.Tokens[1].TokenSymbol.ShouldBe("AELF");
        labsFee.Tokens[1].LabsFee.ShouldBe(123456);

        var activeResult = await Query.ActiveAddressAsync(_liquidityRepository, _swapRecordRepository, _recordRepository, _objectMapper, new GetTimeRangeDto()
        {
            TimestampMin = CommitTime.AddMinutes(-1).ToDateTime().ToUnixTimeMilliseconds(),
            TimestampMax = FillTime.AddMinutes(1).ToDateTime().ToUnixTimeMilliseconds()
        });
        
        activeResult.ActiveAddressCount.ShouldBe(2);
        activeResult.NewActiveAddressCount.ShouldBe(2);
        activeResult.ActiveAddresses[0].ShouldBe("2gbvSJdUxUQTBarhfhAC7QyXwz4dKJjUNuaBD1FYRcM6iGv2nK");
        activeResult.ActiveAddresses[1].ShouldBe("2YcGvyn7QPmhvrZ7aaymmb2MDYWhmAks356nV3kUwL8FkGSYeZ");
        
        
        var tradeActiveResult = await Query.TradeActiveAddressAsync(_liquidityRepository, _swapRecordRepository, _recordRepository, _objectMapper, new GetActiveAddressDto()
        {
            TimestampMin = CommitTime.AddMinutes(-1).ToDateTime().ToUnixTimeMilliseconds(),
            TimestampMax = FillTime.AddMinutes(1).ToDateTime().ToUnixTimeMilliseconds(),
            TransactionType = (int)ActiveAddressTransactionType.Swap
        });
        
        tradeActiveResult.ActiveAddressCount.ShouldBe(1);
        tradeActiveResult.ActiveAddresses[0].ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        
        
        tradeActiveResult = await Query.TradeActiveAddressAsync(_liquidityRepository, _swapRecordRepository, _recordRepository, _objectMapper, new GetActiveAddressDto()
        {
            TimestampMin = CommitTime.AddMinutes(-1).ToDateTime().ToUnixTimeMilliseconds(),
            TimestampMax = FillTime.AddMinutes(1).ToDateTime().ToUnixTimeMilliseconds(),
            TransactionType = (int)ActiveAddressTransactionType.Limit
        });
        
        tradeActiveResult.ActiveAddressCount.ShouldBe(1);
        tradeActiveResult.ActiveAddresses[0].ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
    }
    
    
    [Fact]
    public async Task TransactionVolumeAsyncTests()
    {
        var swap = new Awaken.Contracts.Swap.Swap()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolIn = "BTC",
            SymbolOut = "AELF",
            AmountIn = 100,
            AmountOut = 1,
            TotalFee = 15,
            Channel = "test"
        };

        var limitTotalFill = new LimitOrderTotalFilled()
        {
            SymbolIn = "AELF",
            SymbolOut = "BTC",
            AmountInFilled = 1,
            AmountOutFilled = 100,
        };

        var liquidityAdd = new LiquidityAdded()
        {
            SymbolA = "ELF",
            SymbolB = "USDT",
            AmountA = 1,
            AmountB = 10,
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("AAA".HexToByteArray())
        };
        
        var logEventContext = GenerateLogEventContext(swap);
        logEventContext.Block.BlockTime = FillTime.ToDateTime();

        var swapProcessor = GetRequiredService<SwapProcessor>();
        await swapProcessor.ProcessAsync(swap, logEventContext);
        
        var limitTotalFilledProcessor = GetRequiredService<LimitOrderTotalFilledProcessor>();
        await limitTotalFilledProcessor.ProcessAsync(limitTotalFill, logEventContext);
        
        var liquidityAddedProcessor = GetRequiredService<LiquidityAddedProcessor>();
        await liquidityAddedProcessor.ProcessAsync(liquidityAdd, logEventContext);
        
        var transactionVolume = await Query.TransactionVolumeAsync(_liquidityRepository, _swapRecordRepository,
            _objectMapper, new GetTransactionVolumeDto()
            {
                TimestampMin = FillTime.AddMinutes(-1).ToDateTime().ToUnixTimeMilliseconds(),
                TimestampMax = FillTime.AddMinutes(1).ToDateTime().ToUnixTimeMilliseconds()
            });
        transactionVolume.TransactionVolumes.Count.ShouldBe(3);
        transactionVolume.TransactionVolumes[0].TokenSymbol.ShouldBe("AELF");
        transactionVolume.TransactionVolumes[0].Amount.ShouldBe(2);
        transactionVolume.TransactionVolumes[1].TokenSymbol.ShouldBe("ELF");
        transactionVolume.TransactionVolumes[1].Amount.ShouldBe(1);
        transactionVolume.TransactionVolumes[2].TokenSymbol.ShouldBe("USDT");
        transactionVolume.TransactionVolumes[2].Amount.ShouldBe(10);
        transactionVolume.TransactionCount.ShouldBe(3);
        
        
        transactionVolume = await Query.TransactionVolumeAsync(_liquidityRepository, _swapRecordRepository,
            _objectMapper, new GetTransactionVolumeDto()
            {
                TimestampMin = FillTime.AddMinutes(-1).ToDateTime().ToUnixTimeMilliseconds(),
                TimestampMax = FillTime.AddMinutes(1).ToDateTime().ToUnixTimeMilliseconds(),
                TransactionType = (int)TransactionType.Liquidity
            });
        transactionVolume.TransactionVolumes.Count.ShouldBe(2);
        transactionVolume.TransactionVolumes[0].TokenSymbol.ShouldBe("ELF");
        transactionVolume.TransactionVolumes[0].Amount.ShouldBe(1);
        transactionVolume.TransactionVolumes[1].TokenSymbol.ShouldBe("USDT");
        transactionVolume.TransactionVolumes[1].Amount.ShouldBe(10);
        transactionVolume.TransactionCount.ShouldBe(1);
        
        var pairTransactionVolume = await Query.PairTransactionVolumeAsync(_liquidityRepository, _swapRecordRepository,
            _objectMapper, new GetTransactionVolumeDto()
            {
                TimestampMin = FillTime.AddMinutes(-1).ToDateTime().ToUnixTimeMilliseconds(),
                TimestampMax = FillTime.AddMinutes(1).ToDateTime().ToUnixTimeMilliseconds()
            });
        pairTransactionVolume.PairTransactionVolumes.Count.ShouldBe(1);
        pairTransactionVolume.PairTransactionVolumes[0].PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        var tokenMap = pairTransactionVolume.PairTransactionVolumes[0].TokenTransactionVolume;
        tokenMap.TransactionVolumes.Count.ShouldBe(3);
        tokenMap.TransactionVolumes[0].TokenSymbol.ShouldBe("AELF");
        tokenMap.TransactionVolumes[0].Amount.ShouldBe(1);
        tokenMap.TransactionVolumes[1].TokenSymbol.ShouldBe("ELF");
        tokenMap.TransactionVolumes[1].Amount.ShouldBe(1);
        tokenMap.TransactionVolumes[2].TokenSymbol.ShouldBe("USDT");
        tokenMap.TransactionVolumes[2].Amount.ShouldBe(10);
        tokenMap.TransactionCount.ShouldBe(2);
    }
}