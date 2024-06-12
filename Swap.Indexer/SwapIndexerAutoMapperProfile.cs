using AElfIndexer.Client.Handlers;
using AutoMapper;
using Awaken.Contracts.Swap;
using Swap.Indexer.Application.Contracts.Token;
using Swap.Indexer.Entities;
using Swap.Indexer.Entities.Token;
using Swap.Indexer.GraphQL;
using SwapRecord = Swap.Indexer.GraphQL.SwapRecord;

namespace Swap.Indexer;

public class SwapIndexerAutoMapperProfile : Profile
{
    public SwapIndexerAutoMapperProfile()
    {
        CreateMap<LiquidityAdded, LiquidityRecordIndex>();
        CreateMap<LiquidityRemoved, LiquidityRecordIndex>();
        CreateMap<LogEventContext, LiquidityRecordIndex>();
        CreateMap<LogEventContext, UserLiquidityIndex>();
        CreateMap<LiquidityRecordIndex, LiquidityRecordDto>();
        CreateMap<LiquidityRecordIndex, UserLiquidityIndex>();
        CreateMap<UserLiquidityIndex, UserLiquidityDto>();
        CreateMap<SyncRecordIndex, SyncRecordDto>();
        CreateMap<LogEventContext, SyncRecordIndex>();
        CreateMap<Sync, SyncRecordIndex>();
        CreateMap<SwapRecordIndex, SwapRecordDto>();
        CreateMap<Entities.SwapRecord, SwapRecord>();
        CreateMap<LogEventContext, SwapRecordIndex>();
        CreateMap<Awaken.Contracts.Swap.Swap, SwapRecordIndex>();
        CreateMap<LogEventContext, SwapUserTokenIndex>();
        CreateMap<SwapUserTokenIndex, UserTokenDto>();
        CreateMap<TradePairInfoIndex, TradePairInfoDto>();
        CreateMap<TokenRecordIndex, TokenRecordIndexDto>();
        CreateMap<LogEventContext, TradePairInfoIndex>();
        CreateMap<PairCreated, TradePairInfoIndex>();
    }
}