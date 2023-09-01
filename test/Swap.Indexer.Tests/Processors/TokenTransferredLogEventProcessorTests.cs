using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
using Swap.Indexer.Entities;
using Swap.Indexer.GraphQL;
using Swap.Indexer.Processors;
using Swap.Indexer.Tests.Helper;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Swap.Indexer.Tests.Processors;

public class TokenTransferredLogEventProcessorTests : SwapIndexerTests
{
    private readonly IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> _repository;
    private readonly IObjectMapper _objectMapper;

    public TokenTransferredLogEventProcessorTests()
    {
        _repository = GetRequiredService<IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }
    [Fact]
    public async Task SwapTokenTransferredAsyncTests()
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
        var transferred = new Transferred()
        {
            From = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Symbol = "USDT",
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferred.ToLogEvent());
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
            MethodName = "Transferred",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenTransferredLogEventProcessor>();
        await tokenTransferredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        tokenTransferredLogEventProcessor.GetContractAddress(chainId);
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var fromIndex = await _repository.GetAsync(chainId + "-" + transferred.From.ToBase58() + "-" + transferred.Symbol);
        var toIndex = await _repository.GetAsync(chainId + "-" + transferred.To.ToBase58() + "-" + transferred.Symbol);
        fromIndex.Balance.ShouldBe(100);
        toIndex.Balance.ShouldBe(100);
    }
    
    [Fact]
    public async Task SwapTokenCrossChainTransferredAsyncTests()
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
        var transferred = new CrossChainTransferred()
        {
            From = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Symbol = "USDT",
        };
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(transferred.ToLogEvent());
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
            MethodName = "CrossChainTransferred",
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\":\"3\"}" },
                { "ResourceFee", "{\"ELF\":\"3\"}" }
            },
            BlockTime = DateTime.UtcNow
        };
        
        //step3: handle event and write result to blockStateSet
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
        await tokenTransferredLogEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
        tokenTransferredLogEventProcessor.GetContractAddress(chainId);
        
        //step4: save blockStateSet into es
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        await BlockStateSetSaveDataAsync<TransactionInfo>(blockStateSetKeyTransaction);
        await Task.Delay(2000);
        
        //step5: check result
        var fromIndex = await _repository.GetAsync(chainId + "-" + transferred.From.ToBase58() + "-" + transferred.Symbol);
        var toIndex = await _repository.GetAsync(chainId + "-" + transferred.To.ToBase58() + "-" + transferred.Symbol);
        fromIndex.Balance.ShouldBe(100);
        toIndex.Balance.ShouldBe(100);
    } 

    [Fact]
    public async Task QueryTests()
    {
        await SwapTokenTransferredAsyncTests();
        var resultList = await Query.GetUserTokensAsync(_repository, _objectMapper, new GetUserTokenDto
        {
            ChainId = "AELF",
            Address = Address.FromPublicKey("AAA".HexToByteArray()).ToBase58(),
            Symbol = "USDT"
        });
        resultList.Count.ShouldBe(1);
        resultList.First().Symbol.ShouldBe("USDT");
        resultList.First().Balance.ShouldBe(100);
        
        resultList = await Query.GetUserTokensAsync(_repository, _objectMapper, new GetUserTokenDto
        {
            ChainId = "AELF",
            Address = Address.FromPublicKey("BBB".HexToByteArray()).ToBase58()
        });
        resultList.Count.ShouldBe(1);
        resultList.First().Balance.ShouldBe(100);
        resultList.First().Symbol.ShouldBe("USDT");
    }
}