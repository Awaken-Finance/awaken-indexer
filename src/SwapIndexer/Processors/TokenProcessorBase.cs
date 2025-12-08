using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.Types;
using SwapIndexer;

using Microsoft.Extensions.Options;
using SwapIndexer.Entities;
using SwapIndexer.GraphQL;
using SwapIndexer.Providers;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public abstract class TokenProcessorBase<TEvent> : LogEventProcessorBase<TEvent>
    where TEvent : IEvent<TEvent>,new()
{
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();
    protected readonly IAElfDataProvider AElfDataProvider;
    public TokenProcessorBase(
        IAElfDataProvider aElfDataProvider)
    {
        AElfDataProvider = aElfDataProvider;
    }

    public override string GetContractAddress(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.AELF => AwakenSwapConst.MultiTokenContractAddress,
            AwakenSwapConst.tDVV => AwakenSwapConst.MultiTokenContractAddressTDVV,
            AwakenSwapConst.tDVW => AwakenSwapConst.MultiTokenContractAddressTDVW,
            _ => string.Empty
        };
    }

    protected async Task HandleEventBaseAsync(UserTokenDto dto, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, dto.Address, dto.Symbol);
        var index = await GetEntityAsync<SwapUserTokenIndex>(id);
        if (index == null)
        {
            index = new SwapUserTokenIndex()
            {
                Id = id,
                Address = dto.Address,
                Symbol = dto.Symbol,
                ImageUri = await AElfDataProvider.GetTokenUriAsync(GetContractAddress(context.ChainId), context.ChainId, dto.Symbol)
            };
        }
        index.Balance = await AElfDataProvider.GetBalanceAsync(GetContractAddress(context.ChainId), context.ChainId, dto.Symbol, Address.FromBase58(dto.Address));
        await SaveEntityAsync(index);
    }
}

