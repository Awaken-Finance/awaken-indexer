using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;
using SwapIndexer.GraphQL;
using SwapIndexer.Providers;

using Microsoft.Extensions.Options;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class TokenCrossChainReceivedProcessor : TokenProcessorBase<CrossChainReceived>
{
    public TokenCrossChainReceivedProcessor(
        IAElfDataProvider aElfDataProvider,
        IObjectMapper objectMapper) : base(aElfDataProvider)
    {
    }

    public override async Task ProcessAsync(CrossChainReceived eventValue, LogEventContext context)
    {
        var userToken = new UserTokenDto
        {
            Address = eventValue.From.ToBase58(),
            Symbol = eventValue.Symbol
        };
        await HandleEventBaseAsync(userToken, context);
        userToken.Address = eventValue.To.ToBase58();
        await HandleEventBaseAsync(userToken, context);
    }
}