using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Swap;
using Force.DeepCloner;
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
public sealed class SyncRecordProcessorTests : SwapIndexerTests
{
    private readonly IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> _recordRepository;
    private readonly IObjectMapper _objectMapper;

    public SyncRecordProcessorTests()
    {
        _recordRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task SyncAsyncTests()
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
        var sync = new Sync()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            SymbolA = "AELF",
            SymbolB = "BTC",
            ReserveA = 100,
            ReserveB = 1
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(sync.ToLogEvent());
        logEventInfo.BlockHeight = blockHeight;
        logEventInfo.ChainId = chainId;
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
            MethodName = "Sync",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var syncEventProcessor = GetRequiredService<SyncProcessor>();
        await syncEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        syncEventProcessor.GetContractAddress(chainId).ShouldBe("XXXXXX");

        var syncEventProcessor2 = GetRequiredService<SyncProcessor2>();
        syncEventProcessor2.GetContractAddress(chainId).ShouldBe("XXXXXX2");
        
        var syncEventProcessor3 = GetRequiredService<SyncProcessor3>();
        syncEventProcessor3.GetContractAddress(chainId).ShouldBe("XXXXXX3");
        
        var syncEventProcessor4 = GetRequiredService<SyncProcessor4>();
        syncEventProcessor4.GetContractAddress(chainId).ShouldBe("XXXXXX4");
        
        var syncEventProcessor5 = GetRequiredService<SyncProcessor5>();
        syncEventProcessor5.GetContractAddress(chainId).ShouldBe("XXXXXX5");
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        
        //step5: check result
        var recordData = await _recordRepository.GetAsync($"{chainId}-{transactionId}-{blockHeight}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.SymbolA.ShouldBe("AELF");
        recordData.SymbolB.ShouldBe("BTC");
        recordData.ReserveA.ShouldBe(100);
        recordData.ReserveB.ShouldBe(1);
        recordData.Timestamp.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.BlockTime));
        
        var result = await Query.SyncRecordAsync(_recordRepository, _objectMapper, new GetSyncRecordDto
        {
            SkipCount = 0,
            MaxResultCount = 100,
            ChainId = "AELF",
            PairAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            SymbolA = "A",
            SymbolB = "B",
            ReserveA = 20,
            ReserveB = 10,
            Timestamp = 11
        });
        var dto = new GetSyncRecordDto
        {
            ChainId = "AELF",
            PairAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            SymbolA = "AELF",
            SymbolB = "BTC",
            ReserveA = 100,
            ReserveB = 1,
            Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.BlockTime)
        };
        result.TotalCount.ShouldBe(1);
        result.Data.First().PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe(dto.ChainId);
        result.Data.First().SymbolA.ShouldBe(dto.SymbolA);
        result.Data.First().SymbolB.ShouldBe(dto.SymbolB);
        result.Data.First().ReserveA.ShouldBe(dto.ReserveA);
        result.Data.First().ReserveB.ShouldBe(dto.ReserveB);
        result.Data.First().BlockHeight.ShouldBe(100);
        result.Data.First().Timestamp.ShouldBe(dto.Timestamp);
        
        result = await Query.SyncRecordAsync(_recordRepository, _objectMapper, new GetSyncRecordDto
        {
            SkipCount = 1,
            MaxResultCount = 100,
            ChainId = "AELF",
            PairAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            SymbolA = "A",
            SymbolB = "B",
            ReserveA = 20,
            ReserveB = 10,
            Timestamp = 11
        });
        result.Data.Count.ShouldBe(0);
        
        result = await Query.SyncRecordAsync(_recordRepository, _objectMapper, new GetSyncRecordDto
        {
            SkipCount = 0,
            MaxResultCount = 0,
            ChainId = "AELF",
            PairAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            SymbolA = "A",
            SymbolB = "B",
            ReserveA = 20,
            ReserveB = 10,
            Timestamp = 11
        });
        result.Data.Count.ShouldBe(0);
        
        var ret = await Query.GetSyncRecordsAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 101
        });
        ret.Count.ShouldBe(1);
        ret.First().PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        ret.First().ChainId.ShouldBe("AELF");
        ret.First().SymbolA.ShouldBe("AELF");
        ret.First().SymbolB.ShouldBe("BTC");
        ret.First().ReserveA.ShouldBe(100);
        ret.First().ReserveB.ShouldBe(1);
        ret.First().BlockHeight.ShouldBe(100);
        
        ret = await Query.GetSyncRecordsAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 101,
            SkipCount = 1
        });
        ret.Count.ShouldBe(0);
        
        ret = await Query.GetSyncRecordsAsync(_recordRepository, _objectMapper, new GetChainBlockHeightDto
        {
            ChainId = "AELF",
            StartBlockHeight = 1,
            EndBlockHeight = 101,
            MaxResultCount = 0
        });
        ret.Count.ShouldBe(0);

    }
}