using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.GraphQL;
using Swap.Indexer.Options;
using Swap.Indexer.Providers;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class TokenBurnedEventProcessor : TokenProcessorBase<Burned>
{
    public TokenBurnedEventProcessor(ILogger<TokenBurnedEventProcessor> logger,
        IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfDataProvider aElfDataProvider,
        IObjectMapper objectMapper) : base(logger, repository, contractInfoOptions, aElfDataProvider, objectMapper)
    {
    }

    protected override async Task HandleEventAsync(Burned eventValue, LogEventContext context)
    {
        _logger.Info("received Burned:" + eventValue + ",context:" + context);
        var userToken = new UserTokenDto
        {
            Address = eventValue.Burner.ToBase58(),
            Symbol = eventValue.Symbol
        };
        await HandleEventBaseAsync(userToken, context);
    }
}