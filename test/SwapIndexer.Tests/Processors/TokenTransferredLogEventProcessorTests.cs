using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AeFinder.App.TestBase;
using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElf.Types;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
using SwapIndexer.Entities;
using SwapIndexer.GraphQL;
using SwapIndexer.Processors;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace SwapIndexer.Tests.Processors;

public class TokenTransferredLogEventProcessorTests : SwapIndexerTestBase
{
    private readonly IReadOnlyRepository<SwapUserTokenIndex> _repository;
    private readonly IObjectMapper _objectMapper;

    public TokenTransferredLogEventProcessorTests()
    {
        _repository = GetRequiredService<IReadOnlyRepository<SwapUserTokenIndex>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }
    [Fact]
    public async Task SwapTokenTransferredAsyncTests()
    {
        //step1: create blockStateSet
        const string chainId = "AELF";
        
        var transferred = new Transferred()
        {
            From = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Symbol = "USDT",
        };
        var logEventContext = GenerateLogEventContext(transferred);
       
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenTransferredLogEventProcessor>();
        await tokenTransferredLogEventProcessor.ProcessAsync(transferred, logEventContext);
        tokenTransferredLogEventProcessor.GetContractAddress(chainId);
        
        var fromIndex = await SwapIndexerTestHelper.GetEntityAsync(_repository, chainId + "-" + transferred.From.ToBase58() + "-" + transferred.Symbol);
        var toIndex = await SwapIndexerTestHelper.GetEntityAsync(_repository, chainId + "-" + transferred.To.ToBase58() + "-" + transferred.Symbol);
        fromIndex.Balance.ShouldBe(100);
        fromIndex.Address.ShouldBe(transferred.From.ToBase58());
        fromIndex.Symbol.ShouldBe("USDT");
        toIndex.Balance.ShouldBe(100);
        toIndex.Address.ShouldBe(transferred.To.ToBase58());
        toIndex.Symbol.ShouldBe("USDT");
        fromIndex.Metadata.ChainId.ShouldBe(ChainId);
        fromIndex.Metadata.Block.BlockHeight.ShouldBe(logEventContext.Block.BlockHeight);
        fromIndex.Metadata.Block.BlockHash.ShouldBe(logEventContext.Block.BlockHash);
    }
    
    [Fact]
    public async Task SwapTokenCrossChainTransferredAsyncTests()
    {
        const string chainId = "AELF";
        
        var transferred = new CrossChainTransferred()
        {
            From = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Symbol = "USDT",
        };
        var logEventContext = GenerateLogEventContext(transferred);
        
        var tokenTransferredLogEventProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
        await tokenTransferredLogEventProcessor.ProcessAsync(transferred, logEventContext);
        tokenTransferredLogEventProcessor.GetContractAddress(chainId);
      
        var fromIndex = await SwapIndexerTestHelper.GetEntityAsync(_repository, chainId + "-" + transferred.From.ToBase58() + "-" + transferred.Symbol);
        var toIndex = await SwapIndexerTestHelper.GetEntityAsync(_repository, chainId + "-" + transferred.To.ToBase58() + "-" + transferred.Symbol);
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