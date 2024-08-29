using AElf.Types;
using AeFinder.Sdk;
using AElf.Contracts.MultiToken;
using Awaken.Contracts.Hooks;
using Awaken.Contracts.Order;
using Google.Protobuf;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
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
    private readonly LimitOrderTotalFilledProcessor _limitOrderTotalFilledProcessor;
    
    const string ChainId = "AELF";

    public SwapProcessorTests()
    {
        _recordRepository = GetRequiredService<IReadOnlyRepository<SwapRecordIndex>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
        _swapProcessor = GetRequiredService<SwapProcessor>();
        _hooksProcessor = GetRequiredService<HooksTransactionCreatedProcessor>();
        _limitOrderTotalFilledProcessor = GetRequiredService<LimitOrderTotalFilledProcessor>();
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
        recordData.Metadata.Block.BlockTime.ToUnixTimeSeconds().ShouldBe(logEventContext.Block.BlockTime.ToUnixTimeSeconds());
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
        recordData.Metadata.Block.BlockTime.ToUnixTimeSeconds().ShouldBe(logEventContext.Block.BlockTime.ToUnixTimeSeconds());
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
}