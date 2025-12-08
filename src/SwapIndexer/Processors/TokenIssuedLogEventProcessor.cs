using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;
using SwapIndexer.GraphQL;
using SwapIndexer.Providers;

using Microsoft.Extensions.Options;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class TokenIssuedEventProcessor : TokenProcessorBase<Issued>
{
    public TokenIssuedEventProcessor(
        IAElfDataProvider aElfDataProvider) : base(aElfDataProvider)
    {
    }

    public override async Task ProcessAsync(Issued eventValue, LogEventContext context)
    {
        var userToken = new UserTokenDto
        {
            Address = eventValue.To.ToBase58(),
            Symbol = eventValue.Symbol
        };
        await HandleEventBaseAsync(userToken, context);
    }
}