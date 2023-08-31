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

public class TokenIssuedEventProcessor : TokenProcessorBase<Issued>
{
    public TokenIssuedEventProcessor(ILogger<TokenIssuedEventProcessor> logger,
        IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfDataProvider aElfDataProvider,
        IObjectMapper objectMapper) : base(logger, repository, contractInfoOptions, aElfDataProvider, objectMapper)
    {
    }

    protected override async Task HandleEventAsync(Issued eventValue, LogEventContext context)
    {
        _logger.Info("received Issued:" + eventValue + ",context:" + context);
        var userToken = new UserTokenDto
        {
            Address = eventValue.To.ToBase58(),
            Symbol = eventValue.Symbol
        };
        await HandleEventAsync(userToken, context);
    }
}