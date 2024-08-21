using AeFinder.Sdk.Processor;
using AutoMapper;
using Awaken.Contracts.Order;
using Awaken.Contracts.Swap;
using SwapIndexer.Application.Contracts.Token;
using SwapIndexer.Entities;
using SwapIndexer.Entities.Token;
using SwapIndexer.GraphQL;
using SwapIndexer.Entities;
using Volo.Abp.AutoMapper;

namespace SwapIndexer;

public class SwapIndexerProfile : Profile
{
    public SwapIndexerProfile()
    {
        CreateMap<LiquidityAdded, LiquidityRecordIndex>();
        CreateMap<LiquidityRemoved, LiquidityRecordIndex>();
        // CreateMap<LogEventContext, LiquidityRecordIndex>();
        // CreateMap<LogEventContext, UserLiquidityIndex>();
        CreateMap<LiquidityRecordIndex, UserLiquidityIndex>();
        // CreateMap<LogEventContext, SyncRecordIndex>();
        CreateMap<Sync, SyncRecordIndex>();
        CreateMap<SwapIndexer.Entities.SwapRecord, SwapIndexer.GraphQL.SwapRecord>();
        // CreateMap<LogEventContext, SwapRecordIndex>().Ignore(t => t.MethodName);
        CreateMap<Awaken.Contracts.Swap.Swap, SwapRecordIndex>();
        // CreateMap<LogEventContext, SwapUserTokenIndex>();
        // CreateMap<LogEventContext, TradePairInfoIndex>();
        CreateMap<PairCreated, TradePairInfoIndex>();
        
        // CreateMap<LogEventContext, LimitOrderIndex>();
        CreateMap<LimitOrderCreated, LimitOrderIndex>()
            .ForMember(
                destination => destination.CommitTime,
                opt => opt.MapFrom(source => DateTimeHelper.ToUnixTimeMilliseconds(source.CommitTime.ToDateTime())))
            .ForMember(
                destination => destination.Deadline,
                opt => opt.MapFrom(source => DateTimeHelper.ToUnixTimeMilliseconds(source.Deadline.ToDateTime())))
            .ForMember(destination => destination.Maker,
                opt => opt.MapFrom(source => source.Maker.ToBase58()));
        
        
        CreateMap<LiquidityRecordIndex, LiquidityRecordDto>()
            .ForMember(res => res.ChainId, opt => opt.MapFrom(res => res.Metadata.ChainId))
            .ForMember(res => res.BlockHeight, opt => opt.MapFrom(res => res.Metadata.Block.BlockHeight));
        
        CreateMap<UserLiquidityIndex, UserLiquidityDto>()
            .ForMember(res => res.ChainId, opt => opt.MapFrom(res => res.Metadata.ChainId));
        CreateMap<SyncRecordIndex, SyncRecordDto>()
            .ForMember(res => res.ChainId, opt => opt.MapFrom(res => res.Metadata.ChainId))
            .ForMember(res => res.BlockHeight, opt => opt.MapFrom(res => res.Metadata.Block.BlockHeight));
        CreateMap<SwapRecordIndex, SwapRecordDto>()
            .ForMember(res => res.ChainId, opt => opt.MapFrom(res => res.Metadata.ChainId))
            .ForMember(res => res.BlockHeight, opt => opt.MapFrom(res => res.Metadata.Block.BlockHeight));
        CreateMap<SwapUserTokenIndex, UserTokenDto>()
            .ForMember(res => res.ChainId, opt => opt.MapFrom(res => res.Metadata.ChainId));
        CreateMap<TradePairInfoIndex, TradePairInfoDto>()
            .ForMember(res => res.ChainId, opt => opt.MapFrom(res => res.Metadata.ChainId))
            .ForMember(res => res.BlockHeight, opt => opt.MapFrom(res => res.Metadata.Block.BlockHeight));
        CreateMap<TokenRecordIndex, TokenRecordIndexDto>();
        CreateMap<LimitOrderIndex, LimitOrderDto>()
            .ForMember(res => res.ChainId, opt => opt.MapFrom(res => res.Metadata.ChainId));
    }
}