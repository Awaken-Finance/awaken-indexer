using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;
using SwapIndexer.GraphQL;
using SwapIndexer.Providers;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class TokenBurnedEventProcessor : TokenProcessorBase<Burned>
{
    public TokenBurnedEventProcessor(
        IAElfDataProvider aElfDataProvider,
        IObjectMapper objectMapper) : base(aElfDataProvider)
    {
    }

    public override async Task ProcessAsync(Burned eventValue, LogEventContext context)
    {
        var userToken = new UserTokenDto
        {
            Address = eventValue.Burner.ToBase58(),
            Symbol = eventValue.Symbol
        };
        await HandleEventBaseAsync(userToken, context);
    }
}