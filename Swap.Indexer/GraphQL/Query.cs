using System.Linq.Expressions;
using AElfIndexer.Client;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains;
using AElfIndexer.Grains.Grain.Client;
using AElfIndexer.Grains.State.Client;
using GraphQL;
using Nest;
using Orleans;
using Swap.Indexer.Entities;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.GraphQL;

public class Query
{
    private const string TIMESTAMP = "timestamp";
    private const string TRADEPAIR = "tradepair";
    private const string LPTOKENAMOUNT = "lptokenamount";
    
    [Name("getLiquidityRecords")]
    public static async Task<List<LiquidityRecordDto>> GetLiquidityRecordsAsync(
        [FromServices] IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetPullLiquidityRecordDto dto
        )
    {
        dto.Validate();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<LiquidityRecordIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i
            => i.Field(f => f.ChainId).Value(dto.ChainId)));
        if (dto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }

        if (dto.EndBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }

        QueryContainer Filter(QueryContainerDescriptor<LiquidityRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortType: SortOrder.Ascending, sortExp: o => o.BlockHeight, skip: dto.SkipCount, limit: dto.MaxResultCount);
        return objectMapper.Map<List<LiquidityRecordIndex>, List<LiquidityRecordDto>>(result.Item2);
    }
    
    [Name("liquidityRecord")]
    public static async Task<LiquidityRecordPageResultDto> LiquidityRecordAsync(
        [FromServices] IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetLiquidityRecordDto dto
        )
    {
        dto.Validate();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<LiquidityRecordIndex>, QueryContainer>>();

        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.ChainId).Value(dto.ChainId)));
        }

        if (!string.IsNullOrEmpty(dto.Address))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Address).Value(dto.Address)));
        }

        if (dto.Pair != null)
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Pair).Value(dto.Pair)));
        }

        if (dto.Type.HasValue)
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Type).Value(dto.Type.Value)));
        }

        if (dto.Token0 != null)
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Token0).Value(dto.Token0)));
        }
        if (dto.Token1 != null)
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Token1).Value(dto.Token1)));
        }

        if (dto.TokenSymbol != null)
        {
            mustQuery.Add(q => q.Bool(i => i.Should(
                s => s.Wildcard(w =>
                    w.Field(f => f.Token0).Value($"*{dto.TokenSymbol.ToUpper()}*")),
                s => s.Wildcard(w =>
                    w.Field(f => f.Token1).Value($"*{dto.TokenSymbol.ToUpper()}*")))));
        }

        if (dto.TransactionHash != null)
        {
            mustQuery.Add(q => q.Match(m => m.Field(f => f.TransactionHash).Query(dto.TransactionHash)));
        }

        if (dto.TimestampMin > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.Timestamp).GreaterThanOrEquals(dto.TimestampMin)));
        }

        if (dto.TimestampMax > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.Timestamp).LessThanOrEquals(dto.TimestampMax)));
        }

        QueryContainer Filter(QueryContainerDescriptor<LiquidityRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var sorting = GetSorting(dto.Sorting);
        var result = await repository.GetListAsync(Filter, skip: dto.SkipCount, 
            limit: dto.MaxResultCount, sortType: sorting.Item1, sortExp: sorting.Item2);
        var dataList = objectMapper.Map<List<LiquidityRecordIndex>, List<LiquidityRecordDto>>(result.Item2);
        return new LiquidityRecordPageResultDto()
        {
            TotalCount = result.Item1,
            Data = dataList
        };
    }

    private static Tuple<SortOrder, Expression<Func<LiquidityRecordIndex, object>>> GetSorting(string sorting)
    {
        var result = new Tuple<SortOrder, Expression<Func<LiquidityRecordIndex, object>>>(SortOrder.Descending,
            o => o.Timestamp);
        if (string.IsNullOrWhiteSpace(sorting)) return result;
        
        var sortingArray = sorting.Trim().ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
        switch (sortingArray.Length)
        {
            case 1:
                switch (sortingArray[0])
                {
                    case TIMESTAMP:
                        result = new Tuple<SortOrder, Expression<Func<LiquidityRecordIndex, object>>>(SortOrder.Ascending,
                            o => o.Timestamp);
                        break;
                    case TRADEPAIR:
                        result = new Tuple<SortOrder, Expression<Func<LiquidityRecordIndex, object>>>(SortOrder.Ascending,
                            o => o.Token0);
                        break;
                }
                break;
            case 2:
                switch (sortingArray[0])
                {
                    case TIMESTAMP:
                        result = new Tuple<SortOrder, Expression<Func<LiquidityRecordIndex, object>>>(
                            GetSortOrder(sortingArray[1]),
                            o => o.Timestamp);
                        break;
                    case TRADEPAIR:
                        result = new Tuple<SortOrder, Expression<Func<LiquidityRecordIndex, object>>>(
                            GetSortOrder(sortingArray[1]),
                            o => o.Token0);
                        break;
                }
                break;
        }

        return result;
    }

    [Name("userLiquidity")]
    public static async Task<UserLiquidityPageResultDto> UserLiquidityAsync(
        [FromServices] IAElfIndexerClientEntityRepository<UserLiquidityIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetUserLiquidityDto dto
    )
    {
        dto.Validate();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<UserLiquidityIndex>, QueryContainer>>();
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.ChainId).Value(dto.ChainId)));
        }

        if (!string.IsNullOrEmpty(dto.Address))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Address).Value(dto.Address)));
        }

        QueryContainer Filter(QueryContainerDescriptor<UserLiquidityIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        var sorting = GetUserLiquiditySorting(dto.Sorting);
        var result = await repository.GetListAsync(Filter,  skip: dto.SkipCount,
            limit: dto.MaxResultCount, sortType: sorting.Item1, sortExp: sorting.Item2);
        var dataList = objectMapper.Map<List<UserLiquidityIndex>, List<UserLiquidityDto>>(result.Item2);
        return new UserLiquidityPageResultDto()
        {
            Data = dataList,
            TotalCount = result.Item1
        };
    }
    
    private static Tuple<SortOrder, Expression<Func<UserLiquidityIndex, object>>> GetUserLiquiditySorting(string sorting)
    {
        var result = new Tuple<SortOrder, Expression<Func<UserLiquidityIndex, object>>>(SortOrder.Ascending,
            o => o.Timestamp);
        if (string.IsNullOrWhiteSpace(sorting)) return result;
        
        var sortingArray = sorting.Trim().ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
        switch (sortingArray.Length)
        {
            case 1:
                switch (sortingArray[0])
                {
                    case TIMESTAMP:
                        result = new Tuple<SortOrder, Expression<Func<UserLiquidityIndex, object>>>(SortOrder.Ascending,
                            o => o.Timestamp);
                        break;
                    case LPTOKENAMOUNT:
                        result = new Tuple<SortOrder, Expression<Func<UserLiquidityIndex, object>>>(SortOrder.Ascending,
                            o => o.LpTokenAmount);
                        break;
                }
                break;
            case 2:
                switch (sortingArray[0])
                {
                    case TIMESTAMP:
                        result = new Tuple<SortOrder, Expression<Func<UserLiquidityIndex, object>>>(
                            GetSortOrder(sortingArray[1]),
                            o => o.Timestamp);
                        break;
                    case LPTOKENAMOUNT:
                        result = new Tuple<SortOrder, Expression<Func<UserLiquidityIndex, object>>>(GetSortOrder(sortingArray[1]),
                            o => o.LpTokenAmount);
                        break;
                }
                break;
        }

        return result;
    }

    private static SortOrder GetSortOrder(string sort)
    {
        var sortLower = sort.ToLower();
        return  sortLower == SwapIndexerConst.Asc || sortLower == SwapIndexerConst.Ascend ? SortOrder.Ascending : SortOrder.Descending;
    }

    [Name("syncState")]
    public static async Task<SyncStateDto> SyncStateAsync(
        [FromServices] IClusterClient clusterClient, 
        [FromServices] IAElfIndexerClientInfoProvider clientInfoProvider,
        GetSyncStateDto dto)
    {
        var version = clientInfoProvider.GetVersion();
        var clientId = clientInfoProvider.GetClientId();
        var blockStateSetInfoGrain =
            clusterClient.GetGrain<IBlockStateSetInfoGrain>(
                GrainIdHelper.GenerateGrainId("BlockStateSetInfo", clientId, dto.ChainId, version));
        var confirmedHeight = await blockStateSetInfoGrain.GetConfirmedBlockHeight(dto.FilterType);
        return new SyncStateDto
        {
            ConfirmedBlockHeight = confirmedHeight
        };
    }
    
    [Name("getUserTokens")]
    public static async Task<List<UserTokenDto>> GetUserTokensAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SwapUserTokenIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetUserTokenDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SwapUserTokenIndex>, QueryContainer>>();
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.ChainId).Value(dto.ChainId)));
        }

        if (!string.IsNullOrEmpty(dto.Address))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Address).Value(dto.Address)));
        }

        if (dto.Symbol != null)
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Symbol).Value(dto.Symbol)));
        }

        QueryContainer Filter(QueryContainerDescriptor<SwapUserTokenIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        var result = await repository.GetListAsync(Filter);
        return objectMapper.Map<List<SwapUserTokenIndex>, List<UserTokenDto>>(result.Item2);
    }
    
    [Name("syncRecord")]
    public static async Task<SyncRecordPageResultDto> SyncRecordAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetSyncRecordDto dto
    )
    {
        dto.Validate();

        var mustQuery = new List<Func<QueryContainerDescriptor<SyncRecordIndex>, QueryContainer>>();

        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.ChainId).Value(dto.ChainId)));
        }


        if (!string.IsNullOrEmpty(dto.PairAddress))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.PairAddress).Value(dto.PairAddress)));
        }

        if (dto.TimestampMax > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.Timestamp).LessThanOrEquals(dto.TimestampMax)));
        }

        QueryContainer Filter(QueryContainerDescriptor<SyncRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        var result = await repository.GetListAsync(Filter,  skip: dto.SkipCount,
            limit: dto.MaxResultCount, sortType: SortOrder.Descending, sortExp: o => o.Timestamp);
        var dataList = objectMapper.Map<List<SyncRecordIndex>, List<SyncRecordDto>>(result.Item2);
        return new SyncRecordPageResultDto()
        {
            Data = dataList,
            TotalCount = result.Item1
        };
    }
    
    [Name("getSyncRecords")]
    public static async Task<List<SyncRecordDto>> GetSyncRecordsAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetChainBlockHeightDto dto
    )
    {
        dto.Validate();

        var mustQuery = new List<Func<QueryContainerDescriptor<SyncRecordIndex>, QueryContainer>>();
        
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.ChainId).Value(dto.ChainId)));
        }
        

        if (dto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }

        if (dto.EndBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }

        QueryContainer Filter(QueryContainerDescriptor<SyncRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, skip: dto.SkipCount, limit: dto.MaxResultCount, sortType: SortOrder.Ascending, sortExp: o => o.BlockHeight);
        return objectMapper.Map<List<SyncRecordIndex>, List<SyncRecordDto>>(result.Item2);
    }
    
    [Name("swapRecord")]
    public static async Task<SwapRecordPageResultDto> SwapRecordAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetSwapRecordDto getSwapRecordDto
    )
    {
        getSwapRecordDto.Validate();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<SwapRecordIndex>, QueryContainer>>();

        if (!string.IsNullOrEmpty(getSwapRecordDto.ChainId))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.ChainId).Value(getSwapRecordDto.ChainId)));
        }

        if (!string.IsNullOrEmpty(getSwapRecordDto.PairAddress))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.PairAddress).Value(getSwapRecordDto.PairAddress)));
        }

        QueryContainer Filter(QueryContainerDescriptor<SwapRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        var result = await repository.GetListAsync(Filter,  skip: getSwapRecordDto.SkipCount,
            limit: getSwapRecordDto.MaxResultCount, sortType: SortOrder.Ascending, sortExp: o => o.Timestamp);
        var dataList = objectMapper.Map<List<SwapRecordIndex>, List<SwapRecordDto>>(result.Item2);
        return new SwapRecordPageResultDto()
        {
            Data = dataList,
            TotalCount = result.Item1
        };
    }
    
    [Name("getSwapRecords")]
    public static async Task<List<SwapRecordDto>> GetSwapRecordsAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetChainBlockHeightDto dto
    )
    {
        dto.Validate();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<SwapRecordIndex>, QueryContainer>>();

        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.ChainId).Value(dto.ChainId)));
        }

        if (dto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }

        if (dto.EndBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }

        QueryContainer Filter(QueryContainerDescriptor<SwapRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, skip: dto.SkipCount, limit: dto.MaxResultCount, sortType: SortOrder.Ascending, sortExp: o => o.BlockHeight);
        return objectMapper.Map<List<SwapRecordIndex>, List<SwapRecordDto>>(result.Item2);
    }

    [Name("TradePairInfo")]
    public static async Task<List<TradePairInfoDto>> TradePairInfoAsync(
        [FromServices] IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTradePairInfoDto getTradePairInfoDto
    )
    {
        getTradePairInfoDto.Validate();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<TradePairInfoIndex>, QueryContainer>>();
        
        if (!string.IsNullOrEmpty(getTradePairInfoDto.ChainId))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.ChainId).Value(getTradePairInfoDto.ChainId)));
        }

        if (!string.IsNullOrEmpty(getTradePairInfoDto.Token0Symbol))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Token0Symbol).Value(getTradePairInfoDto.Token0Symbol)));
        }

        if (!string.IsNullOrEmpty(getTradePairInfoDto.Token1Symbol))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Token1Symbol).Value(getTradePairInfoDto.Token1Symbol)));
        }

        QueryContainer Filter(QueryContainerDescriptor<TradePairInfoIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, skip: getTradePairInfoDto.SkipCount,
            limit: getTradePairInfoDto.MaxResultCount);
        return objectMapper.Map<List<TradePairInfoIndex>, List<TradePairInfoDto>>(result.Item2);
    }
    
    [Name("getTradePairInfoList")]
    public static async Task<TradePairInfoDtoPageResultDto> GetTradePairInfoListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTradePairInfoDto getTradePairInfoDto
    )
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<TradePairInfoIndex>, QueryContainer>>();

        if (!string.IsNullOrEmpty(getTradePairInfoDto.Id))
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.Id).Value(getTradePairInfoDto.Id)));
        }
        
        if (!string.IsNullOrEmpty(getTradePairInfoDto.ChainId))
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(getTradePairInfoDto.ChainId)));
        }
        
        if (!string.IsNullOrEmpty(getTradePairInfoDto.Token0Symbol))
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.Token0Symbol).Value(getTradePairInfoDto.Token0Symbol)));
        }

        if (!string.IsNullOrEmpty(getTradePairInfoDto.Token1Symbol))
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.Token1Symbol).Value(getTradePairInfoDto.Token1Symbol)));
        }

        if (!string.IsNullOrEmpty(getTradePairInfoDto.Address))
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.Address).Value(getTradePairInfoDto.Address)));
        }

        if (getTradePairInfoDto.FeeRate > 0)
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.FeeRate).Value(getTradePairInfoDto.FeeRate)));
        }

        if (!string.IsNullOrEmpty(getTradePairInfoDto.TokenSymbol))
        {
            mustQuery.Add(q => q.Term(t => t.Field(f => f.Token0Symbol).Value(getTradePairInfoDto.TokenSymbol))
                               || q.Term(t => t.Field(f => f.Token0Symbol).Value(getTradePairInfoDto.TokenSymbol)));
        }
        
        if (getTradePairInfoDto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.BlockHeight).GreaterThanOrEquals(getTradePairInfoDto.StartBlockHeight)));
        }

        if (getTradePairInfoDto.EndBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i
                => i.Field(f => f.BlockHeight).LessThanOrEquals(getTradePairInfoDto.EndBlockHeight)));
        }

        QueryContainer Filter(QueryContainerDescriptor<TradePairInfoIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter,  skip: getTradePairInfoDto.SkipCount, limit: getTradePairInfoDto.MaxResultCount);
        var dataList = objectMapper.Map<List<TradePairInfoIndex>, List<TradePairInfoDto>>(result.Item2);
        return new TradePairInfoDtoPageResultDto()
        {
            Data = dataList,
            TotalCount = result.Item1
        };
    }
}