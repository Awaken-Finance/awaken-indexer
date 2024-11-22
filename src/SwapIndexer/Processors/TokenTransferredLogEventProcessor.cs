using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;

using Microsoft.Extensions.Options;
using SwapIndexer.Entities;
using SwapIndexer.GraphQL;
using SwapIndexer.Providers;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public class TokenTransferredLogEventProcessor : TokenProcessorBase<Transferred>
{
    public TokenTransferredLogEventProcessor(
        IAElfDataProvider aElfDataProvider) : base(aElfDataProvider)
    {
    }
    
    public override async Task ProcessAsync(Transferred eventValue, LogEventContext context)
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