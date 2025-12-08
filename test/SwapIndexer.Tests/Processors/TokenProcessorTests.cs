using AeFinder.App.TestBase;
using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Nethereum.Hex.HexConvertors.Extensions;
using Shouldly;
using SwapIndexer.Entities;
using SwapIndexer.Processors;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace SwapIndexer.Tests.Processors;


public sealed class TokenProcessorTests : SwapIndexerTestBase
{
    private readonly IReadOnlyRepository<SwapUserTokenIndex> _repository;
    private readonly IObjectMapper _objectMapper;

    public TokenProcessorTests()
    {
        _repository = GetRequiredService<IReadOnlyRepository<SwapUserTokenIndex>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async Task TokenAsyncTests()
    {
        var tokenBurnedEvent = new Burned
        {
            Burner = Address.FromPublicKey("AAA".HexToByteArray()),
            Symbol = "AELF"
        };
        var tokenCrossChainReceivedEvent = new CrossChainReceived()
        {
            From = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Symbol = "AELF"
        };
        var tokenCrossChainTransferredEvent = new CrossChainTransferred()
        {
            From = Address.FromPublicKey("AAA".HexToByteArray()),
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Symbol = "AELF-ETH"
        };
        var tokenIssuedEvent = new AElf.Contracts.MultiToken.Issued()
        {
            To = Address.FromPublicKey("BBB".HexToByteArray()),
            Symbol = "AELF-2"
        };
        
        var logEventContext = GenerateLogEventContext(tokenBurnedEvent);
       
        var tokenBurnedEventProcessor = GetRequiredService<TokenBurnedEventProcessor>();
        await tokenBurnedEventProcessor.ProcessAsync(tokenBurnedEvent, logEventContext);
        
        var tokenCrossChainReceivedProcessor = GetRequiredService<TokenCrossChainReceivedProcessor>();
        await tokenCrossChainReceivedProcessor.ProcessAsync(tokenCrossChainReceivedEvent, logEventContext);
        
        var tokenCrossChainTransferredProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
        await tokenCrossChainTransferredProcessor.ProcessAsync(tokenCrossChainTransferredEvent, logEventContext);
        
        var tokenIssuedEventProcessor = GetRequiredService<TokenIssuedEventProcessor>();
        await tokenIssuedEventProcessor.ProcessAsync(tokenIssuedEvent, logEventContext);
    }
}