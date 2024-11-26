using AElf.Types;
using AeFinder.Sdk;
using AElf.Contracts.MultiToken;
using Awaken.Contracts.Hooks;
using Awaken.Contracts.Order;
using Google.Protobuf;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
using Swap.Indexer.Processors;
using SwapIndexer.Entities;
using SwapIndexer.GraphQL;
using SwapIndexer.Processors;
using SwapIndexer;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace SwapIndexer.Tests.Processors;


public sealed class SwapProcessorTests : SwapIndexerTestBase
{
    private readonly IReadOnlyRepository<SwapRecordIndex> _recordRepository;
    private readonly IObjectMapper _objectMapper;
    private readonly SwapProcessor _swapProcessor;
    private readonly HooksTransactionCreatedProcessor _hooksProcessor;
    private readonly LabsFeeChargedProcessor _labsFeeProcessor;
    private readonly LimitOrderTotalFilledProcessor _limitOrderTotalFilledProcessor;
    
    const string ChainId = "AELF";

    public SwapProcessorTests()
    {
        _recordRepository = GetRequiredService<IReadOnlyRepository<SwapRecordIndex>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
        _swapProcessor = GetRequiredService<SwapProcessor>();
        _hooksProcessor = GetRequiredService<HooksTransactionCreatedProcessor>();
        _limitOrderTotalFilledProcessor = GetRequiredService<LimitOrderTotalFilledProcessor>();
        _labsFeeProcessor = GetRequiredService<LabsFeeChargedProcessor>();
    }
    
    [Fact]
    public async Task SwapProcessorContractAddressAsyncTests()
    {
        _swapProcessor.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("2YnkipJ9mty5r6tpTWQAwnomeeKUT7qCWLHKaSeV1fejYEyCdX");
        _swapProcessor.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("hyiwdsbDnyoG1uZiw2JabQ4tLiWT6yAuDfNBFbHhCZwAqU1os");
        _swapProcessor.GetContractAddress("notexist").ShouldBe(string.Empty);
        
        var swapProcessor2 = GetRequiredService<SwapProcessor2>();
        swapProcessor2.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("fGa81UPViGsVvTM13zuAAwk1QHovL3oSqTrCznitS4hAawPpk");
        swapProcessor2.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("JvDB3rguLJtpFsovre8udJeXJLhsV1EPScGz2u1FFneahjBQm");
        swapProcessor2.GetContractAddress("notexist").ShouldBe(string.Empty);
        
        var swapProcessor3 = GetRequiredService<SwapProcessor3>();
        swapProcessor3.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("LzkrbEK2zweeuE4P8Y23BMiFY2oiKMWyHuy5hBBbF1pAPD2hh");
        swapProcessor3.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("83ju3fGGnvQzCmtjApUTwvBpuLQLQvt5biNMv4FXCvWKdZgJf");
        swapProcessor3.GetContractAddress("notexist").ShouldBe(string.Empty);
        
        var swapProcessor4 = GetRequiredService<SwapProcessor4>();
        swapProcessor4.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("EG73zzQqC8JencoFEgCtrEUvMBS2zT22xoRse72XkyhuuhyTC");
        swapProcessor4.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("2q7NLAr6eqF4CTsnNeXnBZ9k4XcmiUeM61CLWYaym6WsUmbg1k");
        swapProcessor4.GetContractAddress("notexist").ShouldBe(string.Empty);
        
        var swapProcessor5 = GetRequiredService<SwapProcessor5>();
        swapProcessor5.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("23dh2s1mXnswi4yNW7eWNKWy7iac8KrXJYitECgUctgfwjeZwP");
        swapProcessor5.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("UYdd84gLMsVdHrgkr3ogqe1ukhKwen8oj32Ks4J1dg6KH9PYC");
        swapProcessor5.GetContractAddress("notexist").ShouldBe(string.Empty);
    }

    [Fact]
    public async Task SwapAsyncTests()
    {
        const string blockHash = "DefaultBlockHash";
        const string transactionId = "DefaultTransactionId";
        const long blockHeight = 100;
        
        var swap = new Awaken.Contracts.Swap.Swap()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolIn = "AELF",
            SymbolOut = "BTC",
            AmountIn = 100,
            AmountOut = 1,
            TotalFee = 15,
            Channel = "test"
        };
        var logEventContext = GenerateLogEventContext(swap);
        logEventContext.Transaction.TransactionId = transactionId;
        logEventContext.Block.BlockHeight = blockHeight;
       
        await _swapProcessor.ProcessAsync(swap, logEventContext);
       
        var recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.Sender.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        recordData.TransactionHash.ShouldBe(transactionId);
        recordData.Timestamp.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.Block.BlockTime));
        recordData.AmountOut.ShouldBe(1);
        recordData.AmountIn.ShouldBe(100);
        recordData.TotalFee.ShouldBe(15);
        recordData.SymbolOut.ShouldBe("BTC");
        recordData.SymbolIn.ShouldBe("AELF");
        recordData.Channel.ShouldBe("test");
        recordData.Metadata.ChainId.ShouldBe(ChainId);
        recordData.Metadata.Block.BlockHeight.ShouldBe(logEventContext.Block.BlockHeight);
        recordData.Metadata.Block.BlockHash.ShouldBe(logEventContext.Block.BlockHash);
        
        var result = await Query.SwapRecordAsync(_recordRepository, _objectMapper, new GetSwapRecordDto
        {
            SkipCount = 0,
            MaxResultCount = 100,
            ChainId = "AELF",
            PairAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
        });
        
        result.TotalCount.ShouldBe(1);
        result.Data.First().PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe("AELF");
        result.Data.First().Sender.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        result.Data.First().TransactionHash.ShouldBe(transactionId);
        result.Data.First().Timestamp.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.Block.BlockTime));
        result.Data.First().AmountOut.ShouldBe(1);
        result.Data.First().AmountIn.ShouldBe(100);
        result.Data.First().SymbolOut.ShouldBe("BTC");
        result.Data.First().SymbolIn.ShouldBe("AELF");
        result.Data.First().Channel.ShouldBe("test");
        
        result = await Query.SwapRecordAsync(_recordRepository, _objectMapper, new GetSwapRecordDto
        {
            SkipCount = 1,
            MaxResultCount = 100,
            ChainId = "AELF",
            PairAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            Sender = "AA",
            TransactionHash = "aa",
            Timestamp = 11,
            AmountOut = 100,
            AmountIn = 99,
            SymbolOut = "AA",
            SymbolIn = "BB",
            Channel = "test"
        });
        result.Data.Count.ShouldBe(0);
        
        var ret = await Query.GetSwapRecordsAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 101
        });
        ret.Count.ShouldBe(1);
        ret.First().PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        ret.First().ChainId.ShouldBe("AELF");
        ret.First().BlockHeight.ShouldBe(100);
        ret.First().Sender.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        ret.First().TransactionHash.ShouldBe(transactionId);
        ret.First().Timestamp.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.Block.BlockTime));
        ret.First().AmountOut.ShouldBe(1);
        ret.First().AmountIn.ShouldBe(100);
        ret.First().TotalFee.ShouldBe(15);
        ret.First().SymbolOut.ShouldBe("BTC");
        ret.First().SymbolIn.ShouldBe("AELF");
        
        ret = await Query.GetSwapRecordsAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 101,
            SkipCount = 1
        });
        ret.Count.ShouldBe(0);
        
        ret = await Query.GetSwapRecordsAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 101,
            MaxResultCount = 0
        });
        ret.Count.ShouldBe(0);
        
        await _swapProcessor.ProcessAsync(swap, logEventContext); 
        
        ret = await Query.GetSwapRecordsAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 101
        });
        ret.Count.ShouldBe(1);
        ret.First().SwapRecords.Count.ShouldBe(0);
        
        var swap2 = new Awaken.Contracts.Swap.Swap()
        {
            Pair = Address.FromPublicKey("AAA2".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolIn = "BTC",
            SymbolOut = "ETH",
            AmountIn = 100,
            AmountOut = 1,
            TotalFee = 15,
            Channel = "test"
        };
        logEventContext = GenerateLogEventContext(swap2);
        logEventContext.Block.BlockHeight = blockHeight;
        logEventContext.ChainId= ChainId;
        logEventContext.Block.BlockHash = blockHash;
        logEventContext.Transaction.TransactionId = transactionId;
        
        await _swapProcessor.ProcessAsync(swap2, logEventContext); 
        
        ret = await Query.GetSwapRecordsAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 101
        });
        ret.Count.ShouldBe(1);
        ret.First().SwapRecords.Count.ShouldBe(1);
        ret.First().SwapRecords[0].PairAddress.ShouldBe(swap2.Pair.ToBase58());
        ret.First().SwapRecords[0].AmountIn.ShouldBe(swap2.AmountIn);
        ret.First().SwapRecords[0].AmountOut.ShouldBe(swap2.AmountOut);
        ret.First().SwapRecords[0].SymbolIn.ShouldBe(swap2.SymbolIn);
        ret.First().SwapRecords[0].SymbolOut.ShouldBe(swap2.SymbolOut);
        ret.First().SwapRecords[0].TotalFee.ShouldBe(swap2.TotalFee);
        
        await _swapProcessor.ProcessAsync(swap2, logEventContext); 
        
        ret.Count.ShouldBe(1);
        ret.First().SwapRecords.Count.ShouldBe(1);
    }
    
    [Fact]
    public async Task HooksTransactionCreatedAfterTests()
    {
        const string ChainId = "AELF";
        const string blockHash = "DefaultBlockHash";
        const string transactionId = "DefaultTransactionId";
        const long blockHeight = 100;
       
        var swap = new Awaken.Contracts.Swap.Swap()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolIn = "AELF",
            SymbolOut = "BTC",
            AmountIn = 100,
            AmountOut = 1,
            TotalFee = 15,
            Channel = "test"
        };
        var hooksTransactionCreatedLogEvent = new HooksTransactionCreated()
        {
            Sender = Address.FromPublicKey("DDD".HexToByteArray()),
            MethodName = "SwapTokensForExactTokens"
        };
        
        var logEventContext = GenerateLogEventContext(swap);
        logEventContext.Block.BlockHeight = blockHeight;
        logEventContext.ChainId= ChainId;
        logEventContext.Block.BlockHash = blockHash;
        logEventContext.Transaction.TransactionId = transactionId;
        
        await _swapProcessor.ProcessAsync(swap, logEventContext);
        
        var recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.Sender.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        recordData.MethodName.ShouldBeNull();
        
        await _hooksProcessor.ProcessAsync(hooksTransactionCreatedLogEvent, logEventContext);
        
        recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.Sender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        recordData.MethodName.ShouldBe("SwapTokensForExactTokens");
    }
    
    [Fact]
    public async Task HooksTransactionCreatedBeforeTests()
    {
        const string ChainId = "AELF";
        const string blockHash = "DefaultBlockHash";
        const string previousBlockHash = "DefaultPreviousBlockHash";
        const string transactionId = "DefaultTransactionId";
        const long blockHeight = 100;
       
        var hooksTransactionCreatedLogEvent = new HooksTransactionCreated()
        {
            Sender = Address.FromPublicKey("DDD".HexToByteArray()),
            MethodName = "SwapExactTokensForTokens"
        };
        var swap = new Awaken.Contracts.Swap.Swap()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolIn = "AELF",
            SymbolOut = "BTC",
            AmountIn = 100,
            AmountOut = 1,
            TotalFee = 15,
            Channel = "test"
        };
        var labsFeeCharged = new LabsFeeCharged()
        {
            Amount = 123456,
            Symbol = "BTC"
        };
        
        var logEventContext = GenerateLogEventContext(hooksTransactionCreatedLogEvent);
        logEventContext.Block.BlockHeight = blockHeight;
        logEventContext.ChainId= ChainId;
        logEventContext.Block.BlockHash = blockHash;
        logEventContext.Transaction.TransactionId = transactionId;
        
        await _hooksProcessor.ProcessAsync(hooksTransactionCreatedLogEvent, logEventContext);
        
        var recordData =
            await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.Sender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        recordData.MethodName.ShouldBe("SwapExactTokensForTokens");
        recordData.TransactionHash.ShouldBeNull();
        
        await _swapProcessor.ProcessAsync(swap, logEventContext);
        
        recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.Sender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        recordData.TransactionHash.ShouldBe(transactionId);
        recordData.MethodName.ShouldBe("SwapExactTokensForTokens");

        await _labsFeeProcessor.ProcessAsync(labsFeeCharged, logEventContext);
        recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.Sender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        recordData.TransactionHash.ShouldBe(transactionId);
        recordData.MethodName.ShouldBe("SwapExactTokensForTokens");
        recordData.LabsFee.ShouldBe(123456);
        recordData.LabsFeeSymbol.ShouldBe("BTC");
    }
    
    [Fact]
    public async Task SwapLimitOrderTotalFilledTests()
    {
        //step1: create blockStateSet
        const string ChainId = "AELF";
        const string blockHash = "DefaultBlockHash";
        const string transactionId = "DefaultTransactionId";
        const long blockHeight = 100;
        
        var swapArgs = new SwapExactTokensForTokensInput()
        {
            SwapTokens = { 
                new SwapExactTokensForTokens() {
                    Path = { "USDT", "ELF" },
                    FeeRates = { 5 }
                }
            }
        };
        var hooksTransactionCreatedLogEvent = new HooksTransactionCreated()
        {
            Sender = Address.FromPublicKey("DDD".HexToByteArray()),
            MethodName = "SwapExactTokensForTokens",
            Args = swapArgs.ToByteString()
        };
        var logEventContext = GenerateLogEventContext(hooksTransactionCreatedLogEvent);
        logEventContext.Block.BlockHeight = blockHeight;
        logEventContext.ChainId= ChainId;
        logEventContext.Block.BlockHash = blockHash;
        logEventContext.Transaction.TransactionId = transactionId;
        
        await _hooksProcessor.ProcessAsync(hooksTransactionCreatedLogEvent, logEventContext);
        
        var recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.Sender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        recordData.MethodName.ShouldBe("SwapExactTokensForTokens");
        recordData.TransactionHash.ShouldBeNull();
        recordData.InputArgs.ShouldBe(hooksTransactionCreatedLogEvent.Args.ToBase64());
        
        var limitOrderTotalFilledLogEvent = new LimitOrderTotalFilled()
        {
            Sender = Address.FromPublicKey("DDD".HexToByteArray()),
            SymbolIn = "ELF",
            SymbolOut = "USDT",
            AmountInFilled = 100,
            AmountOutFilled = 10,
        };
        
        await _limitOrderTotalFilledProcessor.ProcessAsync(limitOrderTotalFilledLogEvent, logEventContext);
        
        recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.Sender.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        recordData.SymbolIn.ShouldBe("USDT");
        recordData.SymbolOut.ShouldBe("ELF");
        recordData.AmountIn.ShouldBe(10);
        recordData.AmountOut.ShouldBe(100);
        recordData.IsLimitOrder.ShouldBe(true);
        recordData.InputArgs.ShouldBe(hooksTransactionCreatedLogEvent.Args.ToBase64());
        var swapTokens = SwapExactTokensForTokensInput
            .Parser.ParseFrom(ByteString.FromBase64(recordData.InputArgs)).SwapTokens;
        swapTokens.Count.ShouldBe(1);
        swapTokens[0].Path.Count.ShouldBe(2);
        swapTokens[0].Path[0].ShouldBe("USDT");
        swapTokens[0].Path[1].ShouldBe("ELF");
    }
    
    [Fact]
    public async Task SwapRecordsAsyncTests()
    {
        const string blockHash = "DefaultBlockHash";
        const string transactionId = "DefaultTransactionId";
        const long blockHeight = 100;
        
        var swap = new Awaken.Contracts.Swap.Swap()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolIn = "AELF",
            SymbolOut = "BTC",
            AmountIn = 100,
            AmountOut = 1,
            TotalFee = 15,
            Channel = "test"
        };
        var swap1 = new Awaken.Contracts.Swap.Swap()
        {
            Pair = Address.FromPublicKey("DDD".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()),
            SymbolIn = "BTC",
            SymbolOut = "USDT",
            AmountIn = 1,
            AmountOut = 100,
            TotalFee = 10,
            Channel = "test"
        };
        var logEventContext = GenerateLogEventContext(swap);
        logEventContext.Transaction.TransactionId = transactionId;
        logEventContext.Block.BlockHeight = blockHeight;
       
        await _swapProcessor.ProcessAsync(swap, logEventContext);
        await _swapProcessor.ProcessAsync(swap1, logEventContext);
       
        var recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository, $"{ChainId}-{transactionId}-{blockHeight}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.Sender.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        recordData.TransactionHash.ShouldBe(transactionId);
        recordData.Timestamp.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.Block.BlockTime));
        recordData.AmountOut.ShouldBe(1);
        recordData.AmountIn.ShouldBe(100);
        recordData.TotalFee.ShouldBe(15);
        recordData.SymbolOut.ShouldBe("BTC");
        recordData.SymbolIn.ShouldBe("AELF");
        recordData.Channel.ShouldBe("test");
        recordData.Metadata.ChainId.ShouldBe(ChainId);
        recordData.Metadata.Block.BlockHeight.ShouldBe(logEventContext.Block.BlockHeight);
        recordData.Metadata.Block.BlockHash.ShouldBe(logEventContext.Block.BlockHash);
        recordData.SwapRecords.Count.ShouldBe(1);
        recordData.SwapRecords[0].PairAddress.ShouldBe(Address.FromPublicKey("DDD".HexToByteArray()).ToBase58());
        recordData.SwapRecords[0].SymbolIn.ShouldBe("BTC");
        recordData.SwapRecords[0].SymbolOut.ShouldBe("USDT");
        recordData.SwapRecords[0].AmountIn.ShouldBe(1);
        recordData.SwapRecords[0].AmountOut.ShouldBe(100);
        recordData.SwapRecords[0].IsLimitOrder.ShouldBe(false);
        recordData.SwapRecords[0].Channel.ShouldBe("test");
        recordData.SwapRecords[0].TotalFee.ShouldBe(10);
    }
    
    
    [Fact]
    public async Task GetAllSwapRecordsTests()
    {
        var time = DateTime.UtcNow;
        var dataCount = 10;
        for (int i = 0; i < dataCount; i++)
        {
            var swap = new Awaken.Contracts.Swap.Swap()
            {
                Pair = Address.FromPublicKey("AAA".HexToByteArray()),
                To = Address.FromPublicKey("BBB".HexToByteArray()),
                Sender = Address.FromPublicKey("CCC".HexToByteArray()),
                SymbolIn = "AELF",
                SymbolOut = "BTC",
                AmountIn = i+100,
                AmountOut = 1,
                TotalFee = 15,
                Channel = "test"
            };
            var logEventContext = GenerateLogEventContext(swap);
            logEventContext.Transaction.TransactionId = $"0x{i}";
            logEventContext.Block.BlockHeight = i+100;
            logEventContext.Block.BlockTime = time.AddHours(-1);
            
            await _swapProcessor.ProcessAsync(swap, logEventContext);
        }
        
        var emptySwapRecordQueryable = await _recordRepository.GetQueryableAsync();
        emptySwapRecordQueryable = emptySwapRecordQueryable.Where(a => a.Timestamp > DateTimeHelper.ToUnixTimeMilliseconds(time));
        var emptyResult = await Query.GetAllSwapRecords(emptySwapRecordQueryable, 1);
        emptyResult.Count.ShouldBe(0);
        
        var swapRecordQueryable = await _recordRepository.GetQueryableAsync();
        swapRecordQueryable = swapRecordQueryable.Where(a => a.Timestamp <= DateTimeHelper.ToUnixTimeMilliseconds(time));
        var result = await Query.GetAllSwapRecords(swapRecordQueryable, 1);
        
        result.Count.ShouldBe(dataCount);
        for (int i = 0; i < dataCount; i++)
        {
            result[i].TransactionHash.ShouldBe($"0x{i}");
            result[i].AmountIn.ShouldBe(i+100);
        }
    }
}