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

public class TokenTransferredLogEventProcessor : TokenProcessorBase<Transferred>
{
    private readonly IObjectMapper _objectMapper;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IAElfDataProvider _aElfDataProvider;
    private readonly IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> _repository;
    private readonly ILogger<TokenTransferredLogEventProcessor> _logger;

    public TokenTransferredLogEventProcessor(ILogger<TokenTransferredLogEventProcessor> logger,
        IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfDataProvider aElfDataProvider,
        IObjectMapper objectMapper) : base(logger, repository, contractInfoOptions, aElfDataProvider, objectMapper)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _repository = repository;
        _contractInfoOptions = contractInfoOptions.Value;
        _aElfDataProvider = aElfDataProvider;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos.First(o => o.ChainId == chainId).MultiTokenContractAddress;
    }

    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        _logger.Info("received Transferred:" + eventValue + ",context:" + context);
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