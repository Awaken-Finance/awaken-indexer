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

public class TokenCrossChainReceivedProcessor : TokenProcessorBase<CrossChainReceived>
{
    public TokenCrossChainReceivedProcessor(ILogger<TokenCrossChainReceivedProcessor> logger,
        IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfDataProvider aElfDataProvider,
        IObjectMapper objectMapper) : base(logger, repository, contractInfoOptions, aElfDataProvider, objectMapper)
    {
    }

    protected override async Task HandleEventAsync(CrossChainReceived eventValue, LogEventContext context)
    {
        _logger.Info("received CrossChainReceived:" + eventValue + ",context:" + context);
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