using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Swap.Indexer.Providers;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class TokenTransferredLogEventProcessor : AElfLogEventProcessorBase<Transferred, LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IAElfDataProvider _aElfDataProvider;
    private IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> _repository;
    private ILogger<TokenTransferredLogEventProcessor> _logger;

    public TokenTransferredLogEventProcessor(ILogger<TokenTransferredLogEventProcessor> logger,
        IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfDataProvider aElfDataProvider,
        IObjectMapper objectMapper) : base(logger)
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


        var fromId = IdGenerateHelper.GetId(context.ChainId, eventValue.From.ToBase58(), eventValue.Symbol);
        var fromIndex = await _repository.GetFromBlockStateSetAsync(fromId, context.ChainId);
        fromIndex ??= new SwapUserTokenIndex()
        {
            Id = fromId,
            Address = eventValue.From.ToBase58(),
            Symbol = eventValue.Symbol
        };
        _objectMapper.Map(context, fromIndex);
        fromIndex.Balance =
            await _aElfDataProvider.GetBalanceAsync(context.ChainId, eventValue.Symbol, eventValue.From);
        _logger.Info("SwapUserTokenIndex from:" + fromIndex);
        await _repository.AddOrUpdateAsync(fromIndex);

        var toId = IdGenerateHelper.GetId(context.ChainId, eventValue.To.ToBase58(), eventValue.Symbol);
        var toIndex = await _repository.GetFromBlockStateSetAsync(toId, context.ChainId);
        toIndex ??= new SwapUserTokenIndex()
        {
            Id = toId,
            Address = eventValue.To.ToBase58(),
            Symbol = eventValue.Symbol
        };
        _objectMapper.Map(context, toIndex);
        toIndex.Balance = await _aElfDataProvider.GetBalanceAsync(context.ChainId, eventValue.Symbol, eventValue.To);
        _logger.Info("SwapUserTokenIndex to:" + toIndex);
        await _repository.AddOrUpdateAsync(toIndex);
    }
}