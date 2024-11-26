using System.Runtime.InteropServices.JavaScript;
using AeFinder.App.Logging;
using AeFinder.Sdk;
using AeFinder.Sdk.Logging;
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
    private readonly IReadOnlyRepository<TradePairInfoIndex> _tradepairRepository;
    private readonly SyncProcessor _syncProcessor;
    private readonly IObjectMapper _objectMapper;
    private readonly IAeFinderLogger _logger;

    public SyncRecordProcessorTests()
    {
        _recordRepository = GetRequiredService<IReadOnlyRepository<SyncRecordIndex>>();
        _tradepairRepository = GetRequiredService<IReadOnlyRepository<TradePairInfoIndex>>();
        _syncProcessor = GetRequiredService<SyncProcessor>();
        _objectMapper = GetRequiredService<IObjectMapper>();
        _logger = GetRequiredService<IAeFinderLogger>();
    }

    [Fact]
    public async Task SyncProcessorContractAddressAsyncTests()
    {
        _syncProcessor.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("2YnkipJ9mty5r6tpTWQAwnomeeKUT7qCWLHKaSeV1fejYEyCdX");
        _syncProcessor.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("hyiwdsbDnyoG1uZiw2JabQ4tLiWT6yAuDfNBFbHhCZwAqU1os");
        _syncProcessor.GetContractAddress("notexist").ShouldBe(string.Empty);
        
        var syncProcessor2 = GetRequiredService<SyncProcessor2>();
        syncProcessor2.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("fGa81UPViGsVvTM13zuAAwk1QHovL3oSqTrCznitS4hAawPpk");
        syncProcessor2.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("JvDB3rguLJtpFsovre8udJeXJLhsV1EPScGz2u1FFneahjBQm");
        syncProcessor2.GetContractAddress("notexist").ShouldBe(string.Empty);
        
        var syncProcessor3 = GetRequiredService<SyncProcessor3>();
        syncProcessor3.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("LzkrbEK2zweeuE4P8Y23BMiFY2oiKMWyHuy5hBBbF1pAPD2hh");
        syncProcessor3.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("83ju3fGGnvQzCmtjApUTwvBpuLQLQvt5biNMv4FXCvWKdZgJf");
        syncProcessor3.GetContractAddress("notexist").ShouldBe(string.Empty);
        
        var syncProcessor4 = GetRequiredService<SyncProcessor4>();
        syncProcessor4.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("EG73zzQqC8JencoFEgCtrEUvMBS2zT22xoRse72XkyhuuhyTC");
        syncProcessor4.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("2q7NLAr6eqF4CTsnNeXnBZ9k4XcmiUeM61CLWYaym6WsUmbg1k");
        syncProcessor4.GetContractAddress("notexist").ShouldBe(string.Empty);
        
        var syncProcessor5 = GetRequiredService<SyncProcessor5>();
        syncProcessor5.GetContractAddress(AwakenSwapConst.tDVW).ShouldBe("23dh2s1mXnswi4yNW7eWNKWy7iac8KrXJYitECgUctgfwjeZwP");
        syncProcessor5.GetContractAddress(AwakenSwapConst.tDVV).ShouldBe("UYdd84gLMsVdHrgkr3ogqe1ukhKwen8oj32Ks4J1dg6KH9PYC");
        syncProcessor5.GetContractAddress("notexist").ShouldBe(string.Empty);
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

    [Fact]
    public async Task ReserveTests()
    {
        var now = DateTime.UtcNow;
        var pairAddress = Address.FromPublicKey("AAA".HexToByteArray());
        var pairCreated = new PairCreated()
        {
            Pair = pairAddress,
            SymbolA = "USDT",
            SymbolB = "ELF"
        };
        var logEventContext = GenerateLogEventContext(pairCreated);
        logEventContext.Transaction.TransactionId = "0x1";
        logEventContext.Block.BlockHeight = 100;
        logEventContext.ChainId = AwakenSwapConst.AELF;
        
        await GetRequiredService<PairCreatedProcessor>().ProcessAsync(pairCreated, logEventContext);
        
        var sync = new Sync()
        {
            Pair = Address.FromPublicKey("AAA".HexToByteArray()),
            SymbolA = "USDT",
            SymbolB = "ELF",
            ReserveA = 1000000,
            ReserveB = 1
        };
        logEventContext = GenerateLogEventContext(pairCreated);
        logEventContext.Transaction.TransactionId = "0x2";
        logEventContext.Block.BlockHeight = 100;
        logEventContext.ChainId = AwakenSwapConst.AELF;
        logEventContext.Block.BlockTime = now.AddHours(-1);
        
        await _syncProcessor.ProcessAsync(sync, logEventContext);
        
        var result = await Query.TotalValueLockedAsync(_recordRepository, _tradepairRepository,
            _logger, new GetTotalValueLockedDto()
            {
                ChainId = AwakenSwapConst.AELF,
                Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(now)
            });
        result.Value.ShouldBe(2);
        
        
        var pairReserveResult = await Query.PairReserveAsync(_recordRepository, _tradepairRepository,
            _objectMapper, new GetPairReserveDto()
            {
                ChainId = AwakenSwapConst.AELF,
                SymbolA = "ELF",
                SymbolB = "USDT"
            });
        pairReserveResult.TotalReserveA.ShouldBe(1);
        pairReserveResult.TotalReserveB.ShouldBe(1000000);
        pairReserveResult.TradePairs.Count.ShouldBe(1);
        pairReserveResult.TradePairs[0].Address.ShouldBe(pairAddress.ToBase58());
        pairReserveResult.SyncRecords.Count.ShouldBe(1);
        pairReserveResult.SyncRecords[0].TransactionHash.ShouldBe("0x2");
    }
}