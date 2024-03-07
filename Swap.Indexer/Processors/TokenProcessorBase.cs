using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.Types;
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

public class TokenProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent, LogEventInfo>
    where TEvent : IEvent<TEvent>,new()
{
    private readonly IObjectMapper _objectMapper;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly IAElfDataProvider _aElfDataProvider;
    private IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> _repository;
    protected ILogger<TokenProcessorBase<TEvent>> _logger;
    public TokenProcessorBase(ILogger<TokenProcessorBase<TEvent>> logger,
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

    protected async Task HandleEventAsync(UserTokenDto dto, LogEventContext context)
    {
       
        var id = IdGenerateHelper.GetId(context.ChainId, dto.Address, dto.Symbol);
        var index = await _repository.GetFromBlockStateSetAsync(id, context.ChainId);
        index ??= new SwapUserTokenIndex()
        {
            Id = id,
            Address = dto.Address,
            Symbol = dto.Symbol
        };
        _objectMapper.Map(context, index);
        index.Balance = await _aElfDataProvider.GetBalanceAsync(context.ChainId, dto.Symbol, Address.FromBase58(dto.Address));
        _logger.Info("SwapUserTokenIndex:" + index);
        await _repository.AddOrUpdateAsync(index);
    }
}

