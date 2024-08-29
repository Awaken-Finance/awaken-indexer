using AeFinder.Sdk;
using AElf.Types;
using Awaken.Contracts.Swap;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
using SwapIndexer.Entities;
using SwapIndexer.GraphQL;
using SwapIndexer.Processors;
using Xunit;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Tests.Processors;

public sealed class SyncRecordProcessorTests : SwapIndexerTestBase
{
    private readonly IReadOnlyRepository<SyncRecordIndex> _recordRepository;
    private readonly SyncProcessor _syncProcessor;
    private readonly IObjectMapper _objectMapper;
    
    public SyncRecordProcessorTests()
    {
        _recordRepository = GetRequiredService<IReadOnlyRepository<SyncRecordIndex>>();
        _syncProcessor = GetRequiredService<SyncProcessor>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task SyncAsyncTests()
    {
        //step1: create blockStateSet
        const string chainId = "AELF";
        const string transactionId = "DefaultTransactionId";

        var sync = new Sync()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            SymbolA = "AELF",
            SymbolB = "BTC",
            ReserveA = 100,
            ReserveB = 1
        };
        var logEventContext = GenerateLogEventContext(sync);
        logEventContext.Transaction.TransactionId = transactionId;

        await _syncProcessor.ProcessAsync(sync, logEventContext);


        var recordData = await SwapIndexerTestHelper.GetEntityAsync(_recordRepository,
            $"{chainId}-{transactionId}-{sync.Pair.ToBase58()}");
        recordData.PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        recordData.SymbolA.ShouldBe("AELF");
        recordData.SymbolB.ShouldBe("BTC");
        recordData.ReserveA.ShouldBe(100);
        recordData.ReserveB.ShouldBe(1);
        recordData.TransactionHash.ShouldBe(logEventContext.Transaction.TransactionId);
        recordData.Timestamp.ShouldBe(DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.Block.BlockTime));
        recordData.Metadata.ChainId.ShouldBe(ChainId);
        recordData.Metadata.Block.BlockHeight.ShouldBe(logEventContext.Block.BlockHeight);
        recordData.Metadata.Block.BlockTime.ToUnixTimeSeconds().ShouldBe(logEventContext.Block.BlockTime.ToUnixTimeSeconds());
        recordData.Metadata.Block.BlockHash.ShouldBe(logEventContext.Block.BlockHash);

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
            Timestamp = 11,
            TimestampMax = ((DateTimeOffset)(logEventContext.Block.BlockTime.AddHours(3))).ToUnixTimeMilliseconds()
        });
        var dto = new GetSyncRecordDto
        {
            ChainId = "AELF",
            PairAddress = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            SymbolA = "AELF",
            SymbolB = "BTC",
            ReserveA = 100,
            ReserveB = 1,
            Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(logEventContext.Block.BlockTime)
        };
        result.TotalCount.ShouldBe(1);
        result.Data.First().PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        result.Data.First().ChainId.ShouldBe(dto.ChainId);
        result.Data.First().SymbolA.ShouldBe(dto.SymbolA);
        result.Data.First().SymbolB.ShouldBe(dto.SymbolB);
        // result.Data.First().ReserveA.ShouldBe(dto.ReserveA);
        // result.Data.First().ReserveB.ShouldBe(dto.ReserveB);
        result.Data.First().BlockHeight.ShouldBe(100);
        // result.Data.First().Timestamp.ShouldBe(dto.Timestamp);

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

        var pairSyncRecordsResult = await Query.PairSyncRecordsAsync(_recordRepository, _objectMapper,
            new GetPairSyncRecordsDto
            {
                PairAddresses = new() { Address.FromPublicKey("AAA".HexToByteArray()).ToBase58() }
            });
        pairSyncRecordsResult.Count.ShouldBe(1);
        pairSyncRecordsResult.First().PairAddress.ShouldBe(Address.FromPublicKey("AAA".HexToByteArray()).ToBase58());
        pairSyncRecordsResult.First().SymbolA.ShouldBe("AELF");
        pairSyncRecordsResult.First().SymbolB.ShouldBe("BTC");
        pairSyncRecordsResult.First().ReserveA.ShouldBe(100);
        pairSyncRecordsResult.First().ReserveB.ShouldBe(1);

        pairSyncRecordsResult = await Query.PairSyncRecordsAsync(_recordRepository, _objectMapper,
            new GetPairSyncRecordsDto
            {
                PairAddresses = new()
                {
                    Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
                    Address.FromPublicKey("BBB".HexToByteArray()).ToBase58()
                }
            });
        pairSyncRecordsResult.Count.ShouldBe(1);
    }
}