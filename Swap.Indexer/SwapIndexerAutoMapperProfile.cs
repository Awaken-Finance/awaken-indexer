using AElfIndexer.Client.Handlers;
using AutoMapper;
using Awaken.Contracts.Order;
using Awaken.Contracts.Swap;
using Swap.Indexer.Application.Contracts.Token;
using Swap.Indexer.Entities;
using Swap.Indexer.Entities.Token;
using Swap.Indexer.GraphQL;
using Volo.Abp.AutoMapper;
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
        CreateMap<LogEventContext, SwapRecordIndex>().Ignore(t => t.MethodName);
        CreateMap<Awaken.Contracts.Swap.Swap, SwapRecordIndex>();
        CreateMap<LogEventContext, SwapUserTokenIndex>();
        CreateMap<SwapUserTokenIndex, UserTokenDto>();
        CreateMap<TradePairInfoIndex, TradePairInfoDto>();
        CreateMap<TokenRecordIndex, TokenRecordIndexDto>();
        CreateMap<LogEventContext, TradePairInfoIndex>();
        CreateMap<PairCreated, TradePairInfoIndex>();
        CreateMap<LimitOrderIndex, LimitOrderDto>();
        CreateMap<LogEventContext, LimitOrderIndex>();
        CreateMap<LimitOrderCreated, LimitOrderIndex>()
            .ForMember(
            destination => destination.CommitTime,
            opt => opt.MapFrom(source => DateTimeHelper.ToUnixTimeMilliseconds(source.CommitTime.ToDateTime())))
            .ForMember(
                destination => destination.Deadline,
                opt => opt.MapFrom(source => DateTimeHelper.ToUnixTimeMilliseconds(source.Deadline.ToDateTime())))
            .ForMember(destination => destination.Maker,
                opt => opt.MapFrom(source => source.Maker.ToBase58()));
    }
}