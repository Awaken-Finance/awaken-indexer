using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
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
public sealed class SwapProcessorTests : SwapIndexerTests
{
    private readonly IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> _recordRepository;
    private readonly IObjectMapper _objectMapper;

    public SwapProcessorTests()
    {
        _recordRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task SwapAsyncTests()
    {
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string blockHash = "DefaultBlockHash";
        const string previousBlockHash = "DefaultPreviousBlockHash";
        const string transactionId = "DefaultTransactionId";
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
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(swap.ToLogEvent());
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
            Signature = "AELF",
            Params = "{ \"to\": \"swap\", \"symbol\": \"ELF\", \"amount\": \"100000000000\" }",
            MethodName = "Swap",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var swapProcessor = GetRequiredService<SwapProcessor>();
        await swapProcessor.HandleEventAsync(logEventInfo, logEventContext);
        swapProcessor.GetContractAddress(chainId).ShouldBe("XXXXXX");
        
        var swapProcessor2 = GetRequiredService<SwapProcessor2>();
        swapProcessor2.GetContractAddress(chainId).ShouldBe("XXXXXX2");
        
        var swapProcessor3 = GetRequiredService<SwapProcessor3>();
        swapProcessor3.GetContractAddress(chainId).ShouldBe("XXXXXX3");
        
        var swapProcessor4 = GetRequiredService<SwapProcessor4>();
        swapProcessor4.GetContractAddress(chainId).ShouldBe("XXXXXX4");
        
        var swapProcessor5 = GetRequiredService<SwapProcessor5>();
        swapProcessor5.GetContractAddress(chainId).ShouldBe("XXXXXX5");

        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        
        //step5: check result
        var recordData = await _recordRepository.GetAsync($"{chainId}-{transactionId}-{blockHeight}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.Sender.ShouldBe(Address.FromPublicKey("CCC".HexToByteArray()).ToBase58());
        recordData.TransactionHash.ShouldBe(transactionId);
        recordData.Timestamp.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.BlockTime));
        recordData.AmountOut.ShouldBe(1);
        recordData.AmountIn.ShouldBe(100);
        recordData.SymbolOut.ShouldBe("BTC");
        recordData.SymbolIn.ShouldBe("AELF");
        recordData.Channel.ShouldBe("test");
    
        var result = await Query.SwapRecordAsync(_recordRepository, _objectMapper, new GetSwapRecordDto
        {
            SkipCount = 0,
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
        var dto = new GetSwapRecordDto
        {
            ChainId = "AELF",
            PairAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            Sender = Address.FromPublicKey("CCC".HexToByteArray()).ToBase58(),
            TransactionHash = transactionId,
            Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.BlockTime),
            AmountOut = 1,
            AmountIn = 100,
            SymbolOut = "BTC",
            SymbolIn = "AELF",
            Channel = "test"
        }; 
        result.TotalCount.ShouldBe(1);
        result.Data.First().PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe(dto.ChainId);
        result.Data.First().Sender.ShouldBe(dto.Sender);
        result.Data.First().TransactionHash.ShouldBe(dto.TransactionHash);
        result.Data.First().Timestamp.ShouldBe(dto.Timestamp);
        result.Data.First().AmountOut.ShouldBe(dto.AmountOut);
        result.Data.First().AmountIn.ShouldBe(dto.AmountIn);
        result.Data.First().SymbolOut.ShouldBe(dto.SymbolOut);
        result.Data.First().SymbolIn.ShouldBe(dto.SymbolIn);
        result.Data.First().Channel.ShouldBe(dto.Channel);
        
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
        ret.First().Timestamp.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.BlockTime));
        ret.First().AmountOut.ShouldBe(1);
        ret.First().AmountIn.ShouldBe(100);
        ret.First().TotalFee.ShouldBe(15);
        ret.First().SymbolOut.ShouldBe("BTC");
        ret.First().SymbolIn.ShouldBe("AELF");
    }
}