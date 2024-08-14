using System.Linq.Expressions;
using AElf.CSharp.Core;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains;
using AElfIndexer.Grains.Grain.Client;
using AElfIndexer.Grains.State.Client;
using GraphQL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Nito.AsyncEx;
using Orleans;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Swap.Indexer.Providers;
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
    
    
    [Name("totalValueLocked")]
    public static async Task<TotalValueLockedResultDto> TotalValueLockedAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> repository,
        [FromServices] IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> tradePairRepository,
        [FromServices] IAElfDataProvider aelfDataProvider,
        [FromServices] IObjectMapper objectMapper,
        [FromServices] ILogger<SyncRecordIndex> _logger,
        [FromServices] IOptionsSnapshot<TotalValueLockedOptions> totalValueLockedOptions,
        GetTotalValueLockedDto dto
    )
    {
        var baseToken = totalValueLockedOptions.Value.BaseToken;
        var quoteToken = totalValueLockedOptions.Value.QuoteToken;
        
        _logger.LogInformation($"[TotalValueLockedAsync] input: {dto.ChainId} {dto.Timestamp}");
        
        long quoteDecimal = await aelfDataProvider.GetDecimaleAsync(dto.ChainId, quoteToken);
        long baseDecimal = await aelfDataProvider.GetDecimaleAsync(dto.ChainId, baseToken);
        
        _logger.LogInformation($"[TotalValueLockedAsync] token decimal usdt: {baseDecimal}, elf: {quoteDecimal}");
        
        var tradepairMustQuery = new List<Func<QueryContainerDescriptor<TradePairInfoIndex>, QueryContainer>>();
        
        tradepairMustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        
        QueryContainer TradePairFilter(QueryContainerDescriptor<TradePairInfoIndex> f) =>
            f.Bool(b => b.Must(tradepairMustQuery));

        var tradePairResult = await tradePairRepository.GetListAsync(TradePairFilter,  skip: 0, limit: 10000);
        _logger.LogInformation($"[TotalValueLockedAsync] all trade pair count: {tradePairResult.Item2.Count}");
        
        var tradePairAddresses = new List<string>();
        var standTradePairAddresses = new List<string>();
        try
        {
            foreach (var tradePairInfo in tradePairResult.Item2)
            {
                if (tradePairInfo.Token0Symbol == quoteToken || tradePairInfo.Token1Symbol == quoteToken
                                                             || tradePairInfo.Token0Symbol == baseToken ||
                                                             tradePairInfo.Token1Symbol == baseToken)
                {
                    tradePairAddresses.Add(tradePairInfo.Address);
                }
            
                if (tradePairInfo.Token0Symbol == quoteToken && tradePairInfo.Token1Symbol == baseToken
                    || tradePairInfo.Token0Symbol == baseToken && tradePairInfo.Token1Symbol == quoteToken)
                {
                    standTradePairAddresses.Add(tradePairInfo.Address);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation($"[TotalValueLockedAsync] Exception: {e}");
        }
        
        _logger.LogInformation($"[TotalValueLockedAsync] standTradePairAddresses count: {standTradePairAddresses.Count}");
        _logger.LogInformation($"[TotalValueLockedAsync] quoteTradePairAddresses count: {tradePairAddresses.Count}");
        
        double priceSum = 0.0;
        long count = 0;
        
        foreach (var pairAddress in standTradePairAddresses)
        {
            try
            {
                var mustQuery = new List<Func<QueryContainerDescriptor<SyncRecordIndex>, QueryContainer>>();
        
                mustQuery.Add(q => q.Term(i
                    => i.Field(f => f.ChainId).Value(dto.ChainId)));
        
                mustQuery.Add(q => q.Term(i
                    => i.Field(f => f.PairAddress).Value(pairAddress)));
        
                mustQuery.Add(q => q.Range(i
                    => i.Field(f => f.Timestamp).LessThanOrEquals(dto.Timestamp)));
        
        
                QueryContainer Filter(QueryContainerDescriptor<SyncRecordIndex> f) =>
                    f.Bool(b => b.Must(mustQuery));
                var result = await repository.GetAsync(Filter, sortType: SortOrder.Descending, sortExp: o => o.Timestamp);
                if (result != null)
                {
                    if (result.SymbolA == baseToken)
                    {
                        ++count;
                        priceSum += (result.ReserveA / Math.Pow(10, baseDecimal)) / (result.ReserveB / Math.Pow(10, quoteDecimal));
                    } else if (result.SymbolB == baseToken)
                    {
                        ++count;
                        priceSum += (result.ReserveB / Math.Pow(10, baseDecimal)) / (result.ReserveA / Math.Pow(10, quoteDecimal));
                    }
                }
                _logger.LogInformation($"[TotalValueLockedAsync] cal elf price count: {count}, price sum: {priceSum}");
            }
            catch (Exception e)
            {
                _logger.LogInformation($"[TotalValueLockedAsync] Exception: {e}");
            }
        }

        double priceAvg = 0.0;
        if (count > 0)
        {
            priceAvg = priceSum / count;
        }
         
        _logger.LogInformation($"[TotalValueLockedAsync] cal elf price avg: {priceAvg}");

        double tvl = 0.0;
        foreach (var pairAddress in tradePairAddresses)
        {
            try
            {
                var mustQuery = new List<Func<QueryContainerDescriptor<SyncRecordIndex>, QueryContainer>>();
        
                mustQuery.Add(q => q.Term(i
                    => i.Field(f => f.ChainId).Value(dto.ChainId)));
        
                mustQuery.Add(q => q.Term(i
                    => i.Field(f => f.PairAddress).Value(pairAddress)));
        
                mustQuery.Add(q => q.Range(i
                    => i.Field(f => f.Timestamp).LessThanOrEquals(dto.Timestamp)));
        
        
                QueryContainer Filter(QueryContainerDescriptor<SyncRecordIndex> f) =>
                    f.Bool(b => b.Must(mustQuery));
                var result = await repository.GetAsync(Filter, sortType: SortOrder.Descending, sortExp: o => o.Timestamp);
                if (result != null)
                {
                    double value = 0.0;
                    
                    if (result.SymbolA == baseToken)
                    {
                        value = 2 * result.ReserveA / Math.Pow(10, baseDecimal);
                    } else if (result.SymbolB == baseToken)
                    {
                        value = 2 * result.ReserveB / Math.Pow(10, baseDecimal);
                    } else if (result.SymbolA == quoteToken)
                    {
                        value = 2 * result.ReserveA / Math.Pow(10, quoteDecimal) * priceAvg;
                    } else if (result.SymbolB == quoteToken)
                    {
                        value = 2 * result.ReserveB / Math.Pow(10, quoteDecimal) * priceAvg;
                    }
                    
                    _logger.LogInformation($"[TotalValueLockedAsync] pair: {pairAddress}, tokenA: {result.SymbolA}, tokenB: {result.SymbolB}, reserveA: {result.ReserveA}, reserveB: {result.ReserveB}, value: {value}");
            
                    tvl += value;
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"[TotalValueLockedAsync] Exception: {e}");
            }
        }
        
        _logger.LogInformation($"[TotalValueLockedAsync] chain: {dto.ChainId}, time: {dto.Timestamp} result: {tvl}");

        if (Double.IsNaN(tvl))
        {
            tvl = 0.0;
        }
        
        return new TotalValueLockedResultDto()
        {
            Value = tvl
        };
    }

    [Name("pairSyncRecords")]
    public static async Task<List<SyncRecordDto>> PairSyncRecordsAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetPairSyncRecordsDto dto
    )
    {
        var tasks = dto.PairAddresses.Distinct().Select(t => GetLatestSyncRecordIndexAsync(t, objectMapper, repository));
        var taskResultList = await tasks.WhenAll();
        return taskResultList.Where(t => t != null).ToList();
    }
    
    [Name("pairReserve")]
    public static async Task<PairReserveDto> PairReserveAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> repository,
        [FromServices] IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> tradePairRepository,
        [FromServices] IObjectMapper objectMapper, GetPairReserveDto dto
    )
    {
        
        var pairInfos =
                await GetPairAddressesAsync(dto.SymbolA, dto.SymbolB, objectMapper, tradePairRepository);
        
        var tasks = pairInfos.Select(x=>x.Address).Distinct().Select(t => GetLatestSyncRecordIndexAsync(t, objectMapper, repository));
        var taskResultList = await tasks.WhenAll();
        var syncRecords = taskResultList.Where(t => t != null).ToList();
        var totalReserveA = 0L;
        var totalReserveB = 0L;
        foreach (var syncRecord in syncRecords)
        {
            totalReserveA += syncRecord.SymbolA == dto.SymbolA ? syncRecord.ReserveA : syncRecord.ReserveB;
            totalReserveB += syncRecord.SymbolB == dto.SymbolB ? syncRecord.ReserveB : syncRecord.ReserveA;
        }

        return new PairReserveDto()
        {
            TradePairs = pairInfos,
            SyncRecords = syncRecords,
            TotalReserveA = totalReserveA,
            TotalReserveB = totalReserveB
        };
    }

    private static async Task<SyncRecordDto> GetLatestSyncRecordIndexAsync(string pairAddress, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<SyncRecordIndex, LogEventInfo> repository)
    {
        var query = new List<Func<QueryContainerDescriptor<SyncRecordIndex>, QueryContainer>>
            { q => q.Term(i => i.Field(f => f.PairAddress).Value(pairAddress)) };
        QueryContainer SyncRecordFilter(QueryContainerDescriptor<SyncRecordIndex> f) =>
            f.Bool(b => b.Must(query));

        var recentSyncRecord = await repository.GetListAsync(SyncRecordFilter, sortExp: k => k.Timestamp, sortType: SortOrder.Descending,
            skip:0, limit:1);
        if (recentSyncRecord.Item1 > 0)
        {
            return objectMapper.Map<SyncRecordIndex, SyncRecordDto>(recentSyncRecord.Item2[0]);
        }
        return null;
    }
    
    private static async Task<List<TradePairInfoDto>> GetPairAddressesAsync(string tokenA, string tokenB, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository)
    {
        var query = new List<Func<QueryContainerDescriptor<TradePairInfoIndex>, QueryContainer>>
        {
            q => q.Bool(b => b
                    .Should(
                        s => s.Bool(bb => bb
                            .Must(
                                m => m.Term(t => t.Field(f => f.Token0Symbol).Value(tokenA)),
                                m => m.Term(t => t.Field(f => f.Token1Symbol).Value(tokenB))
                            )
                        ),
                        s => s.Bool(bb => bb
                            .Must(
                                m => m.Term(t => t.Field(f => f.Token0Symbol).Value(tokenB)),
                                m => m.Term(t => t.Field(f => f.Token1Symbol).Value(tokenA))
                            )
                        )
                    )
                    .MinimumShouldMatch(1)
            )
        };

        QueryContainer tradePairFilter(QueryContainerDescriptor<TradePairInfoIndex> f) =>
            f.Bool(b => b.Must(query));

        var tradePairRecord = await repository.GetListAsync(tradePairFilter, sortExp: k => k.BlockHeight, sortType: SortOrder.Descending,
            skip:0, limit:100);
        if (tradePairRecord.Item1 > 0)
        {
            return objectMapper.Map<List<TradePairInfoIndex>, List<TradePairInfoDto>>(tradePairRecord.Item2);
        }
        return null;
    }
    
    [Name("limitOrders")]
    public static async Task<LimitOrderPageResultDto> LimitOrderAsync(
        [FromServices] IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetLimitOrderDto dto
    )
    {
        dto.Validate();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<LimitOrderIndex>, QueryContainer>>();
        if (dto.LimitOrderStatus > 0)
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.LimitOrderStatus).Value((LimitOrderStatus)dto.LimitOrderStatus)));
        }

        if (!string.IsNullOrEmpty(dto.MakerAddress))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Maker).Value(dto.MakerAddress)));
        }
        
        if (!string.IsNullOrEmpty(dto.TokenSymbol))
        {
            mustQuery.Add(q => q.Bool(i => i.Should(
                s => s.Wildcard(w =>
                    w.Field(f => f.SymbolIn).Value($"*{dto.TokenSymbol.ToUpper()}*")),
                s => s.Wildcard(w =>
                    w.Field(f => f.SymbolOut).Value($"*{dto.TokenSymbol.ToUpper()}*")))));
        }
        
        QueryContainer Filter(QueryContainerDescriptor<LimitOrderIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        var result = await repository.GetSortListAsync(Filter,
            sortFunc: s => s.Descending(t => t.Deadline),
            skip: dto.SkipCount,
            limit: dto.MaxResultCount);
        var dataList = objectMapper.Map<List<LimitOrderIndex>, List<LimitOrderDto>>(result.Item2);
        var count = await repository.CountAsync(Filter);
        return new LimitOrderPageResultDto()
        {
            Data = dataList,
            TotalCount = count.Count
        };
    }
    
    [Name("limitOrderDetails")]
    public static async Task<LimitOrderPageResultDto> LimitOrderDetailAsync(
        [FromServices] IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetLimitOrderDetailDto dto
    )
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<LimitOrderIndex>, QueryContainer>>();
        
        if (dto.OrderId > 0)
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.OrderId).Value(dto.OrderId)));
        }

        QueryContainer Filter(QueryContainerDescriptor<LimitOrderIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        var result = await repository.GetListAsync(Filter);
        var dataList = objectMapper.Map<List<LimitOrderIndex>, List<LimitOrderDto>>(result.Item2);
        var count = await repository.CountAsync(Filter);
        return new LimitOrderPageResultDto()
        {
            Data = dataList,
            TotalCount = count.Count
        };
    }
    
    [Name("limitOrderRemainingUnfilled")]
    public static async Task<LimitOrderRemainingUnfilledResultDto> LimitOrderRemainingUnfilledAsync(
        [FromServices] IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        [FromServices] IAElfDataProvider aelfDataProvider,
        [FromServices] ILogger<LimitOrderIndex> logger,
        GetLimitOrderRemainingUnfilledDto dto
    )
    {
        logger.LogInformation($"[LimitOrderRemainingUnfilled] ChainId: {dto.ChainId} MakerAddress: {dto.MakerAddress} TokenSymbol: {dto.TokenSymbol}");
        
        dto.Validate();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<LimitOrderIndex>, QueryContainer>>();
        
        if (!string.IsNullOrEmpty(dto.MakerAddress))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Maker).Value(dto.MakerAddress)));
        }
        
        if (!string.IsNullOrEmpty(dto.TokenSymbol))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.SymbolIn).Value(dto.TokenSymbol)));
        }
        
        mustQuery.Add(q => q.Terms(t => t
            .Field(f => f.LimitOrderStatus)
            .Terms(new[] { LimitOrderStatus.Committed, LimitOrderStatus.PartiallyFilling })
        ));

        QueryContainer Filter(QueryContainerDescriptor<LimitOrderIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        var result = await repository.GetSortListAsync(Filter,
            sortFunc: s => s.Descending(t => t.Deadline),
            skip: 0,
            limit: 10000);
        var dataList = objectMapper.Map<List<LimitOrderIndex>, List<LimitOrderDto>>(result.Item2);

        var amountIn = new BigIntValue(0);
        var filledAmountIn = new BigIntValue(0);
        foreach (var limitOrderDto in dataList)
        {
            amountIn = amountIn.Add(limitOrderDto.AmountIn);
            filledAmountIn = filledAmountIn.Add(limitOrderDto.AmountInFilled);
            logger.LogInformation($"[LimitOrderRemainingUnfilled] amountIn: {amountIn} filledAmountIn: {filledAmountIn}");
        }

        var remainingUnfilled = amountIn.Sub(filledAmountIn);
        
        logger.LogInformation($"[LimitOrderRemainingUnfilled] remainingUnfilled: {remainingUnfilled}");
        
        return new LimitOrderRemainingUnfilledResultDto()
        {
            OrderCount = dataList.Count,
            Value = remainingUnfilled.Value
        };
    }
    
}