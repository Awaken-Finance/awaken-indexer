using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AeFinder.Sdk;
using AElf.CSharp.Core;
using AElf.Types;
using GraphQL;
using SwapIndexer.Entities;
using Volo.Abp.ObjectMapping;
using AeFinder.Sdk.Logging;
using SwapIndexer.GraphQL.Dto;

namespace SwapIndexer.GraphQL;

public class Query
{
    private const string TIMESTAMP = "timestamp";
    private const string TRADEPAIR = "tradepair";
    private const string LPTOKENAMOUNT = "lptokenamount";
    
    [Name("getLiquidityRecords")]
    public static async Task<List<LiquidityRecordDto>> GetLiquidityRecordsAsync(
        [FromServices] IReadOnlyRepository<LiquidityRecordIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetPullLiquidityRecordDto dto
    )
    {
        dto.Validate();
    
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            queryable = queryable.Where(record => record.Metadata.ChainId == dto.ChainId);
        }
    
        if (dto.StartBlockHeight > 0)
        {
            queryable = queryable.Where(record => record.Metadata.Block.BlockHeight >= dto.StartBlockHeight);
        }
    
        if (dto.EndBlockHeight > 0)
        {
            queryable = queryable.Where(record => record.Metadata.Block.BlockHeight <= dto.EndBlockHeight);
        }
        
        var result = queryable.OrderBy(o => o.Metadata.Block.BlockHeight).Skip(dto.SkipCount).Take(dto.MaxResultCount).ToList();
        var dataList = objectMapper.Map<List<LiquidityRecordIndex>, List<LiquidityRecordDto>>(result);
    
        return dataList;
    }

    private static IQueryable<LiquidityRecordIndex> MakeLiquidityRecordSortQuery(string sorting, IQueryable<LiquidityRecordIndex> queryable)
    {
        if (!string.IsNullOrWhiteSpace(sorting))
        {
            var sortingArray = sorting.Trim().ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
        
            switch (sortingArray.Length)
            {
                case 1:
                    switch (sortingArray[0])
                    {
                        case TIMESTAMP:
                            queryable = queryable.OrderBy(o => o.Timestamp);
                            break;
                        case TRADEPAIR:
                            queryable = queryable.OrderBy(o => o.Token0);
                            break;
                    }
                    break;
                case 2:
                    switch (sortingArray[0])
                    {
                        case TIMESTAMP:
                            queryable = GetSortOrder(sortingArray[1]) ? 
                                queryable.OrderBy(o => o.Timestamp) : 
                                queryable.OrderByDescending(o => o.Timestamp);
                            break;
                        case TRADEPAIR:
                            queryable = GetSortOrder(sortingArray[1]) ? 
                                queryable.OrderBy(o => o.Token0) : 
                                queryable.OrderByDescending(o => o.Token0);
                            break;
                    }
                    break;
            }
        }
        else
        {
            queryable = queryable.OrderByDescending(o => o.Timestamp);
        }

        return queryable;
    }
    
    [Name("liquidityRecord")]
    public static async Task<LiquidityRecordPageResultDto> LiquidityRecordAsync(
        [FromServices] IReadOnlyRepository<LiquidityRecordIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetLiquidityRecordDto dto
        )
    {
        dto.Validate();
        
        var queryable = await repository.GetQueryableAsync();
        
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            queryable = queryable.Where(record => record.Metadata.ChainId == dto.ChainId);
        }
        
        if (!string.IsNullOrEmpty(dto.Address))
        {
            queryable = queryable.Where(a => a.Address == dto.Address);
        }
    
        if (!string.IsNullOrEmpty(dto.Pair))
        {
            queryable = queryable.Where(a => a.Pair == dto.Pair);
        }
    
        if (dto.Type.HasValue)
        {
            queryable = queryable.Where(a => a.Type == dto.Type.Value);
        }
    
        if (!string.IsNullOrEmpty(dto.Token0))
        {
            queryable = queryable.Where(a => a.Token0 == dto.Token0);
        }
        if (!string.IsNullOrEmpty(dto.Token1))
        {
            queryable = queryable.Where(a => a.Token1 == dto.Token1);
        }
    
        if (!string.IsNullOrEmpty(dto.TokenSymbol))
        {
            queryable = queryable.Where(a =>
                a.Token0.Contains(dto.TokenSymbol.ToUpper()) ||
                a.Token1.Contains(dto.TokenSymbol.ToUpper()));
        }
    
        if (!string.IsNullOrEmpty(dto.TransactionHash))
        {
            queryable = queryable.Where(a => a.TransactionHash == dto.TransactionHash);
        }
    
        if (dto.TimestampMin > 0)
        {
            queryable = queryable.Where(a => a.Timestamp >= dto.TimestampMin);
        }
    
        if (dto.TimestampMax > 0)
        {
            queryable = queryable.Where(a => a.Timestamp <= dto.TimestampMax);
        }

        queryable = MakeLiquidityRecordSortQuery(dto.Sorting, queryable);
        
        var result = queryable.Skip(dto.SkipCount).Take(dto.MaxResultCount).ToList();
        
        var dataList = objectMapper.Map<List<LiquidityRecordIndex>, List<LiquidityRecordDto>>(result);
        return new LiquidityRecordPageResultDto()
        {
            TotalCount = queryable.Count(),
            Data = dataList
        };
    }
    
    private static bool GetSortOrder(string sort)
    {
        var sortLower = sort.ToLower();
        return  sortLower == AwakenSwapConst.Asc || sortLower == AwakenSwapConst.Ascend;
    }
    
    private static IQueryable<UserLiquidityIndex> MakeUserLiquiditySortQuery(string sorting, IQueryable<UserLiquidityIndex> queryable)
    {
        if (!string.IsNullOrEmpty(sorting))
        {
            var sortingArray = sorting.Trim().ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            switch (sortingArray.Length)
            {
                case 1:
                    switch (sortingArray[0])
                    {
                        case TIMESTAMP:
                            queryable = queryable.OrderBy(o => o.Timestamp);
                            break;
                        case LPTOKENAMOUNT:
                            queryable = queryable.OrderBy(o => o.LpTokenAmount);
                            break;
                    }
                    break;
                case 2:
                    switch (sortingArray[0])
                    {
                        case TIMESTAMP:
                            queryable = GetSortOrder(sortingArray[1]) ? 
                                queryable.OrderBy(o => o.Timestamp) : 
                                queryable.OrderByDescending(o => o.Timestamp);
                            break;
                        case LPTOKENAMOUNT:
                            queryable = GetSortOrder(sortingArray[1]) ? 
                                queryable.OrderBy(o => o.LpTokenAmount) : 
                                queryable.OrderByDescending(o => o.LpTokenAmount);
                            break;
                    }
                    break;
            }
        }
        else
        {
            queryable = queryable.OrderBy(o => o.Timestamp);
        }

        return queryable;
    }
   
    [Name("userLiquidity")]
    public static async Task<UserLiquidityPageResultDto> UserLiquidityAsync(
        [FromServices] IReadOnlyRepository<UserLiquidityIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetUserLiquidityDto dto
    )
    {
        dto.Validate();
    
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            queryable = queryable.Where(record => record.Metadata.ChainId == dto.ChainId);
        }
    
        if (!string.IsNullOrEmpty(dto.Address))
        {
            queryable = queryable.Where(record => record.Address == dto.Address);
        }

        queryable = MakeUserLiquiditySortQuery(dto.Sorting, queryable);
    
        var pagedQuery = queryable.Skip(dto.SkipCount).Take(dto.MaxResultCount);
    
        var result = pagedQuery.ToList();
        var totalCount = queryable.Count();
        var dataList = objectMapper.Map<List<UserLiquidityIndex>, List<UserLiquidityDto>>(result);
    
        return new UserLiquidityPageResultDto()
        {
            Data = dataList,
            TotalCount = totalCount
        };
    }
    
    [Name("getUserTokens")]
    public static async Task<List<UserTokenDto>> GetUserTokensAsync(
        [FromServices] IReadOnlyRepository<SwapUserTokenIndex> repository,
        [FromServices] IObjectMapper objectMapper, GetUserTokenDto dto)
    {
        
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            queryable = queryable.Where(record => record.Metadata.ChainId == dto.ChainId);
        }
    
        if (!string.IsNullOrEmpty(dto.Address))
        {
            queryable = queryable.Where(record => record.Address == dto.Address);
        }
    
        if (dto.Symbol != null)
        {
            queryable = queryable.Where(record => record.Symbol == dto.Symbol);
        }
        
        var result = queryable.ToList();
        return objectMapper.Map<List<SwapUserTokenIndex>, List<UserTokenDto>>(result);
    }
    
    [Name("syncRecord")]
    public static async Task<SyncRecordPageResultDto> SyncRecordAsync(
        [FromServices] IReadOnlyRepository<SyncRecordIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetSyncRecordDto dto
    )
    {
        dto.Validate();
    
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            queryable = queryable.Where(record => record.Metadata.ChainId == dto.ChainId);
        }
    
        if (!string.IsNullOrEmpty(dto.PairAddress))
        {
            queryable = queryable.Where(record => record.PairAddress == dto.PairAddress);
        }
    
        if (dto.TimestampMax > 0)
        {
            queryable = queryable.Where(record => record.Timestamp <= dto.TimestampMax);
        }
        
        var result = queryable.OrderByDescending(o => o.Timestamp).Skip(dto.SkipCount).Take(dto.MaxResultCount).ToList();
    
        var dataList = objectMapper.Map<List<SyncRecordIndex>, List<SyncRecordDto>>(result);
        return new SyncRecordPageResultDto()
        {
            Data = dataList,
            TotalCount = queryable.Count()
        };
    }
    
    [Name("getSyncRecords")]
    public static async Task<List<SyncRecordDto>> GetSyncRecordsAsync(
        [FromServices] IReadOnlyRepository<SyncRecordIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetChainBlockHeightDto dto
    )
    {
        dto.Validate();
    
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            queryable = queryable.Where(record => record.Metadata.ChainId == dto.ChainId);
        }
    
        if (dto.StartBlockHeight > 0)
        {
            queryable = queryable.Where(a => a.Metadata.Block.BlockHeight >= dto.StartBlockHeight);
        }
    
        if (dto.EndBlockHeight > 0)
        {
            queryable = queryable.Where(a => a.Metadata.Block.BlockHeight <= dto.EndBlockHeight);
        }
        
        var result = queryable.OrderBy(o => o.Metadata.Block.BlockHeight).Skip(dto.SkipCount).Take(dto.MaxResultCount).ToList();
    
        return objectMapper.Map<List<SyncRecordIndex>, List<SyncRecordDto>>(result);
    }
    
    [Name("swapRecord")]
    public static async Task<SwapRecordPageResultDto> SwapRecordAsync(
        [FromServices] IReadOnlyRepository<SwapRecordIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetSwapRecordDto getSwapRecordDto
    )
    {
        getSwapRecordDto.Validate();
    
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(getSwapRecordDto.ChainId))
        {
            queryable = queryable.Where(a => a.Metadata.ChainId == getSwapRecordDto.ChainId);
        }
        
        if (!string.IsNullOrEmpty(getSwapRecordDto.PairAddress))
        {
            queryable = queryable.Where(a => a.PairAddress == getSwapRecordDto.PairAddress);
        }
        
        if (!string.IsNullOrEmpty(getSwapRecordDto.Sender))
        {
            queryable = queryable.Where(a => a.Sender == getSwapRecordDto.Sender);
        }
    
        queryable = queryable.OrderBy(o => o.Timestamp);
    
        var pagedQuery = queryable.Skip(getSwapRecordDto.SkipCount).Take(getSwapRecordDto.MaxResultCount);
    
        var result = pagedQuery.ToList();
        var dataList = objectMapper.Map<List<SwapRecordIndex>, List<SwapRecordDto>>(result);
    
        return new SwapRecordPageResultDto()
        {
            Data = dataList,
            TotalCount = queryable.Count()
        };
    }
    
    
    [Name("getSwapRecords")]
    public static async Task<List<SwapRecordDto>> GetSwapRecordsAsync(
        [FromServices] IReadOnlyRepository<SwapRecordIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetChainBlockHeightDto dto
    )
    {
        dto.Validate();
    
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            queryable = queryable.Where(t => t.Metadata.ChainId == dto.ChainId);
        }
    
        if (dto.StartBlockHeight > 0)
        {
            queryable = queryable.Where(a => a.Metadata.Block.BlockHeight >= dto.StartBlockHeight);
        }
    
        if (dto.EndBlockHeight > 0)
        {
            queryable = queryable.Where(a => a.Metadata.Block.BlockHeight <= dto.EndBlockHeight);
        }
        
        var result = queryable
            .OrderBy(record => record.Metadata.Block.BlockHeight)
            .Skip(dto.SkipCount)
            .Take(dto.MaxResultCount)
            .ToList();
    
        return objectMapper.Map<List<SwapRecordIndex>, List<SwapRecordDto>>(result);
    }
    
    
    [Name("TradePairInfo")]
    public static async Task<List<TradePairInfoDto>> TradePairInfoAsync(
        [FromServices] IReadOnlyRepository<TradePairInfoIndex> repository,
        [FromServices] IObjectMapper objectMapper, 
        GetTradePairInfoDto getTradePairInfoDto
    )
    {
        getTradePairInfoDto.Validate();
        
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(getTradePairInfoDto.ChainId))
        {
            queryable = queryable.Where(t => t.Metadata.ChainId == getTradePairInfoDto.ChainId);
        }
    
        if (!string.IsNullOrEmpty(getTradePairInfoDto.Token0Symbol))
        {
            queryable = queryable.Where(t => t.Token0Symbol == getTradePairInfoDto.Token0Symbol);
        }
    
        if (!string.IsNullOrEmpty(getTradePairInfoDto.Token1Symbol))
        {
            queryable = queryable.Where(t => t.Token1Symbol == getTradePairInfoDto.Token1Symbol);
        }
        
        var result = queryable
            .Skip(getTradePairInfoDto.SkipCount)
            .Take(getTradePairInfoDto.MaxResultCount)
            .ToList();
        
        return objectMapper.Map<List<TradePairInfoIndex>, List<TradePairInfoDto>>(result);
    }
    
    
    [Name("getTradePairInfoList")]
    public static async Task<TradePairInfoDtoPageResultDto> GetTradePairInfoListAsync(
        [FromServices] IReadOnlyRepository<TradePairInfoIndex> repository,
        [FromServices] IObjectMapper objectMapper, 
        GetTradePairInfoDto getTradePairInfoDto
    )
    {
        var queryable = await repository.GetQueryableAsync();
        
        if (!string.IsNullOrEmpty(getTradePairInfoDto.Id))
        {
            queryable = queryable.Where(t => t.Id == getTradePairInfoDto.Id);
        }
        
        if (!string.IsNullOrEmpty(getTradePairInfoDto.ChainId))
        {
            queryable = queryable.Where(t => t.Metadata.ChainId == getTradePairInfoDto.ChainId);
        }
        
        if (!string.IsNullOrEmpty(getTradePairInfoDto.Token0Symbol))
        {
            queryable = queryable.Where(t => t.Token0Symbol == getTradePairInfoDto.Token0Symbol);
        }
    
        if (!string.IsNullOrEmpty(getTradePairInfoDto.Token1Symbol))
        {
            queryable = queryable.Where(t => t.Token1Symbol == getTradePairInfoDto.Token1Symbol);
        }
    
        if (!string.IsNullOrEmpty(getTradePairInfoDto.Address))
        {
            queryable = queryable.Where(t => t.Address == getTradePairInfoDto.Address);
        }
    
        if (getTradePairInfoDto.FeeRate > 0)
        {
            queryable = queryable.Where(t => t.FeeRate == getTradePairInfoDto.FeeRate);
        }
    
        if (!string.IsNullOrEmpty(getTradePairInfoDto.TokenSymbol))
        {
            queryable = queryable.Where(t => 
                t.Token0Symbol == getTradePairInfoDto.TokenSymbol || t.Token1Symbol == getTradePairInfoDto.TokenSymbol);
        }
        
        if (getTradePairInfoDto.StartBlockHeight > 0)
        {
            queryable = queryable.Where(a => a.Metadata.Block.BlockHeight >= getTradePairInfoDto.StartBlockHeight);
        }
    
        if (getTradePairInfoDto.EndBlockHeight > 0)
        {
            queryable = queryable.Where(a => a.Metadata.Block.BlockHeight <= getTradePairInfoDto.EndBlockHeight);
        }
        
        var result = queryable
            .Skip(getTradePairInfoDto.SkipCount)
            .Take(getTradePairInfoDto.MaxResultCount)
            .ToList();
        
        var dataList = objectMapper.Map<List<TradePairInfoIndex>, List<TradePairInfoDto>>(result);
        return new TradePairInfoDtoPageResultDto()
        {
            Data = dataList,
            TotalCount = queryable.Count()
        };
    }
    
    private static async Task<SyncRecordIndex> GetLatestSyncRecord(IReadOnlyRepository<SyncRecordIndex> repository, string chainId, string pairAddress, long timestamp)
    {
        var syncQueryable = await repository.GetQueryableAsync();
        syncQueryable = syncQueryable.Where(t => t.Metadata.ChainId == chainId);
        syncQueryable = syncQueryable.Where(t => t.PairAddress == pairAddress);
        syncQueryable = syncQueryable.Where(t => t.Timestamp <= timestamp);
    
        var resultList = syncQueryable
            .OrderByDescending(o => o.Timestamp).Take(1).ToList();
        if (resultList != null && resultList.Count > 0)
        {
            return resultList[0];
        }
    
        return null;
    }
    
    
    [Name("totalValueLocked")]
    public static async Task<TotalValueLockedResultDto> TotalValueLockedAsync(
        [FromServices] IReadOnlyRepository<SyncRecordIndex> repository,
        [FromServices] IReadOnlyRepository<TradePairInfoIndex> tradePairRepository,
        [FromServices] IAeFinderLogger logger,
        GetTotalValueLockedDto dto
    )
    {
        var baseToken = AwakenSwapConst.BaseToken;
        var quoteToken = AwakenSwapConst.QuoteToken;
        long quoteDecimal = AwakenSwapConst.QuoteTokenDecimal;
        long baseDecimal = AwakenSwapConst.BaseTokenDecimal;
        
        // logger.LogDebug($"[TotalValueLockedAsync] input: {dto.ChainId} {dto.Timestamp}");
        // logger.LogDebug($"[TotalValueLockedAsync] token decimal usdt: {baseDecimal}, elf: {quoteDecimal}");
        
        var queryable = await tradePairRepository.GetQueryableAsync();
        
        queryable = queryable.Where(t => t.Metadata.ChainId == dto.ChainId);
        
        var tradePairResult = queryable.Skip(0).Take(10000).ToList();
        // logger.LogDebug($"[TotalValueLockedAsync] all trade pair count: {queryable.Count()}");
        
        var tradePairAddresses = new List<string>();
        var standTradePairAddresses = new List<string>();
        try
        {
            foreach (var tradePairInfo in tradePairResult)
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
            logger.LogInformation($"[TotalValueLockedAsync] Exception: {e}");
        }
        
        // logger.LogDebug($"[TotalValueLockedAsync] standTradePairAddresses count: {standTradePairAddresses.Count}");
        // logger.LogDebug($"[TotalValueLockedAsync] quoteTradePairAddresses count: {tradePairAddresses.Count}");
        
        double priceSum = 0.0;
        long count = 0;
        
        foreach (var pairAddress in standTradePairAddresses)
        {
            try
            {
                var syncIndex = await GetLatestSyncRecord(repository, dto.ChainId, pairAddress, dto.Timestamp);
                if (syncIndex != null)
                {
                    if (syncIndex.SymbolA == baseToken)
                    {
                        ++count;
                        priceSum += (syncIndex.ReserveA / Math.Pow(10, baseDecimal)) / (syncIndex.ReserveB / Math.Pow(10, quoteDecimal));
                    } 
                    else if (syncIndex.SymbolB == baseToken)
                    {
                        ++count;
                        priceSum += (syncIndex.ReserveB / Math.Pow(10, baseDecimal)) / (syncIndex.ReserveA / Math.Pow(10, quoteDecimal));
                    }
                }
                // logger.LogDebug($"[TotalValueLockedAsync] cal elf price count: {count}, price sum: {priceSum}");
            }
            catch (Exception e)
            {
                logger.LogInformation($"[TotalValueLockedAsync] Exception: {e}");
            }
        }
    
        double priceAvg = 0.0;
        if (count > 0)
        {
            priceAvg = priceSum / count;
        }
         
        // logger.LogDebug($"[TotalValueLockedAsync] cal elf price avg: {priceAvg}");
    
        double tvl = 0.0;
        foreach (var pairAddress in tradePairAddresses)
        {
            try
            {
               var syncIndex = await GetLatestSyncRecord(repository, dto.ChainId, pairAddress, dto.Timestamp);
                if (syncIndex != null)
                {
                    double value = 0.0;
                    
                    if (syncIndex.SymbolA == baseToken)
                    {
                        value = 2 * syncIndex.ReserveA / Math.Pow(10, baseDecimal);
                    } else if (syncIndex.SymbolB == baseToken)
                    {
                        value = 2 * syncIndex.ReserveB / Math.Pow(10, baseDecimal);
                    } else if (syncIndex.SymbolA == quoteToken)
                    {
                        value = 2 * syncIndex.ReserveA / Math.Pow(10, quoteDecimal) * priceAvg;
                    } else if (syncIndex.SymbolB == quoteToken)
                    {
                        value = 2 * syncIndex.ReserveB / Math.Pow(10, quoteDecimal) * priceAvg;
                    }
                    
                    tvl += value;
                }
            }
            catch (Exception e)
            {
                logger.LogInformation($"[TotalValueLockedAsync] Exception: {e}");
            }
        }
        
        // logger.LogDebug($"[TotalValueLockedAsync] chain: {dto.ChainId}, time: {dto.Timestamp} result: {tvl}");
    
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
        [FromServices] IReadOnlyRepository<SyncRecordIndex> repository,
        [FromServices] IObjectMapper objectMapper, GetPairSyncRecordsDto dto
    )
    {
        var tasks = dto.PairAddresses.Distinct().Select(t => GetLatestSyncRecordIndexAsync(t, objectMapper, repository));
        var taskResultList = await Task.WhenAll(tasks);
        return taskResultList.Where(t => t != null).ToList();
    }
    
    [Name("pairReserve")]
    public static async Task<PairReserveDto> PairReserveAsync(
        [FromServices] IReadOnlyRepository<SyncRecordIndex> repository,
        [FromServices] IReadOnlyRepository<TradePairInfoIndex> tradePairRepository,
        [FromServices] IObjectMapper objectMapper, GetPairReserveDto dto
    )
    {
        
        var pairInfos =
            await GetPairAddressesAsync(dto.SymbolA, dto.SymbolB, objectMapper, tradePairRepository);
        
        var tasks = pairInfos.Select(x=>x.Address).Distinct().Select(t => GetLatestSyncRecordIndexAsync(t, objectMapper, repository));
        var taskResultList = await Task.WhenAll(tasks);
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
        IReadOnlyRepository<SyncRecordIndex> repository)
    {
        var syncQueryable = await repository.GetQueryableAsync();
        syncQueryable = syncQueryable.Where(t => t.PairAddress == pairAddress);
    
        var recentSyncRecord = syncQueryable
            .OrderByDescending(o => o.Timestamp).Take(1).ToList();
        
        if (recentSyncRecord.Count > 0)
        {
            return objectMapper.Map<SyncRecordIndex, SyncRecordDto>(recentSyncRecord[0]);
        }
        return null;
    }
    
    private static async Task<List<TradePairInfoDto>> GetPairAddressesAsync(string tokenA, string tokenB, IObjectMapper objectMapper,
        IReadOnlyRepository<TradePairInfoIndex> repository)
    {
        var queryable = await repository.GetQueryableAsync();
        queryable = queryable.Where(t => 
            t.Token0Symbol == tokenA && t.Token1Symbol == tokenB
         || t.Token0Symbol == tokenB && t.Token1Symbol == tokenA);
    
        var tradePairRecord = queryable
            .OrderByDescending(k => k.Metadata.Block.BlockHeight)
            .Take(10000)
            .ToList();
        
        if (tradePairRecord.Count > 0)
        {
            return objectMapper.Map<List<TradePairInfoIndex>, List<TradePairInfoDto>>(tradePairRecord);
        }
        return null;
    }
    
    [Name("limitOrders")]
    public static async Task<LimitOrderPageResultDto> LimitOrderAsync(
        [FromServices] IReadOnlyRepository<LimitOrderIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetLimitOrderDto dto
    )
    {
        dto.Validate();
        
        var queryable = await repository.GetQueryableAsync();
        if (dto.LimitOrderStatus > 0)
        {
            queryable = queryable.Where(a => a.LimitOrderStatus == (LimitOrderStatus)dto.LimitOrderStatus);
        }
    
        if (!string.IsNullOrEmpty(dto.MakerAddress))
        {
            queryable = queryable.Where(a => a.Maker == dto.MakerAddress);
        }
        
        if (!string.IsNullOrEmpty(dto.TokenSymbol))
        {
            queryable = queryable.Where(a =>
                a.SymbolIn.Contains(dto.TokenSymbol.ToUpper()) ||
                a.SymbolOut.Contains(dto.TokenSymbol.ToUpper()));
        }
        
        var result = queryable.OrderByDescending(t => t.CommitTime)
            .Skip(dto.SkipCount).Take(dto.MaxResultCount).ToList();
        var dataList = objectMapper.Map<List<LimitOrderIndex>, List<LimitOrderDto>>(result);
        return new LimitOrderPageResultDto()
        {
            Data = dataList,
            TotalCount = queryable.Count()
        };
    }
    
    [Name("limitOrderDetails")]
    public static async Task<LimitOrderPageResultDto> LimitOrderDetailAsync(
        [FromServices] IReadOnlyRepository<LimitOrderIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetLimitOrderDetailDto dto
    )
    {
        var queryable = await repository.GetQueryableAsync();
        
        if (dto.OrderId > 0)
        {
            queryable = queryable.Where(a => a.OrderId == dto.OrderId);
        }
        else
        {
            return new LimitOrderPageResultDto()
            {
                TotalCount = 0
            };
        }
    
        var result = queryable.Take(1).ToList();
        var dataList = objectMapper.Map<List<LimitOrderIndex>, List<LimitOrderDto>>(result);
        return new LimitOrderPageResultDto()
        {
            Data = dataList,
            TotalCount = queryable.Count()
        };
    }
    
    [Name("limitOrderRemainingUnfilled")]
    public static async Task<LimitOrderRemainingUnfilledResultDto> LimitOrderRemainingUnfilledAsync(
        [FromServices] IReadOnlyRepository<LimitOrderIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        [FromServices] IAeFinderLogger logger,
        GetLimitOrderRemainingUnfilledDto dto
    )
    {
        // logger.LogDebug($"[LimitOrderRemainingUnfilled] ChainId: {dto.ChainId} MakerAddress: {dto.MakerAddress} TokenSymbol: {dto.TokenSymbol}");
        
        var queryable = await repository.GetQueryableAsync();
        
        if (!string.IsNullOrEmpty(dto.MakerAddress))
        {
            queryable = queryable.Where(a => a.Maker == dto.MakerAddress);
        }
        
        if (!string.IsNullOrEmpty(dto.TokenSymbol))
        {
            queryable = queryable.Where(a => a.SymbolIn == dto.TokenSymbol);
        }
    
        queryable = queryable.Where(a =>
            a.LimitOrderStatus == LimitOrderStatus.Committed
            || a.LimitOrderStatus == LimitOrderStatus.PartiallyFilling);
        
        var result = queryable.OrderBy(t=>t.CommitTime).Skip(0).Take(10000).ToList();
        
        var dataList = objectMapper.Map<List<LimitOrderIndex>, List<LimitOrderDto>>(result);
    
        var amountIn = new BigIntValue(0);
        var filledAmountIn = new BigIntValue(0);
        var orderCount = 0;
        foreach (var limitOrderDto in dataList)
        {
            if (limitOrderDto.LimitOrderStatus == LimitOrderStatus.Committed
                || limitOrderDto.LimitOrderStatus == LimitOrderStatus.PartiallyFilling)
            {
                if (limitOrderDto.Deadline < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                {
                    continue;
                }   
            }
    
            orderCount++;
            amountIn = amountIn.Add(limitOrderDto.AmountIn);
            filledAmountIn = filledAmountIn.Add(limitOrderDto.AmountInFilled);
        }
    
        var remainingUnfilled = amountIn.Sub(filledAmountIn);
        
        // logger.LogDebug($"[LimitOrderRemainingUnfilled] remainingUnfilled: {remainingUnfilled}");
        
        return new LimitOrderRemainingUnfilledResultDto()
        {
            OrderCount = orderCount,
            Value = remainingUnfilled.Value
        };
    }

    private static async Task<List<SwapRecordIndex>> GetAllSwapRecords(IQueryable<SwapRecordIndex> queryable)
    { 
        var swapRecordIndices = new List<SwapRecordIndex>(); 
        var pageNumber = 1; 
        var pageSize = 10000; 
        var currentPageSwapRecordIndices = new List<SwapRecordIndex>();
        do
        {
            currentPageSwapRecordIndices = queryable.OrderByDescending(t => t.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            swapRecordIndices.AddRange(currentPageSwapRecordIndices);
            pageNumber++;

        } while (currentPageSwapRecordIndices.Count == pageSize);

        return swapRecordIndices;
    }
    
    private static async Task<List<LimitOrderIndex>> GetAllLimitRecords(IQueryable<LimitOrderIndex> queryable)
    { 
        var limitOrderIndices = new List<LimitOrderIndex>(); 
        int pageSize = 10000; 
        int pageNumber = 1; 
        var currentPageResults = new List<LimitOrderIndex>();
        do
        {
            currentPageResults = queryable.OrderByDescending(t => t.CommitTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            limitOrderIndices.AddRange(currentPageResults);
            pageNumber++;

        } while (currentPageResults.Count == pageSize);

        return limitOrderIndices;
    }
    
    [Name("labsFee")]
    public static async Task<LabsFeeResultDto> LabsFeeAsync(
        [FromServices] IReadOnlyRepository<LimitOrderIndex> repository,
        [FromServices] IReadOnlyRepository<SwapRecordIndex> swapRepository,
        [FromServices] IObjectMapper objectMapper,
        GetTimeRangeDto dto
    )
    {
        var queryable = await repository.GetQueryableAsync();
        queryable = queryable.Where(a => a.LimitOrderStatus != LimitOrderStatus.Committed);

        var limitOrderIndices = await GetAllLimitRecords(queryable);
        
        var tokenFeeMap = new Dictionary<string, double>();
        
        foreach (var limitOrderIndex in limitOrderIndices)
        {
            foreach (var fillRecord in limitOrderIndex.FillRecords)
            {
                if (fillRecord.TotalFee == 0)
                {
                    continue;
                }
                if (dto.TimestampMin > 0 && fillRecord.TransactionTime <= dto.TimestampMin)
                {
                    continue;
                }
                if (dto.TimestampMax > 0 && fillRecord.TransactionTime > dto.TimestampMax)
                {
                    continue;
                }
                
                if (!tokenFeeMap.ContainsKey(limitOrderIndex.SymbolOut))
                {
                    tokenFeeMap.Add(limitOrderIndex.SymbolOut, 0);
                }
                tokenFeeMap[limitOrderIndex.SymbolOut] += fillRecord.TotalFee;
            }
        }
        
        var swapRecordQueryable = await swapRepository.GetQueryableAsync();
        
        if (dto.TimestampMin > 0)
        {
            swapRecordQueryable = swapRecordQueryable.Where(a => a.Timestamp > dto.TimestampMin);
        }
        if (dto.TimestampMax > 0)
        {
            swapRecordQueryable = swapRecordQueryable.Where(a => a.Timestamp <= dto.TimestampMax);
        }

        swapRecordQueryable = swapRecordQueryable.Where(a => a.LabsFee > 0);
        swapRecordQueryable = swapRecordQueryable.Where(a => a.LabsFeeSymbol != null);

        var swapRecordIndices = await GetAllSwapRecords(swapRecordQueryable);
        
        foreach (var swapRecordIndex in swapRecordIndices)
        {
            if (swapRecordIndex.LabsFee > 0)
            {
                if (!tokenFeeMap.ContainsKey(swapRecordIndex.LabsFeeSymbol))
                {
                    tokenFeeMap.Add(swapRecordIndex.LabsFeeSymbol, 0);
                }
                tokenFeeMap[swapRecordIndex.LabsFeeSymbol] += swapRecordIndex.LabsFee;
            }
        }

        var result = new LabsFeeResultDto();
        foreach (var kv in tokenFeeMap)
        {
            result.Tokens.Add(new TokenLabsFeeDto()
            {
                TokenSymbol = kv.Key,
                LabsFee = kv.Value
            });
        }

        return result;
    }

    [Name("getLimitOrders")]
    public static async Task<List<LimitOrderDto>> GetLimitOrdersAsync(
        [FromServices] IReadOnlyRepository<LimitOrderIndex> repository,
        [FromServices] IObjectMapper objectMapper,
        GetChainBlockHeightDto dto
    )
    {
        dto.Validate();
    
        var queryable = await repository.GetQueryableAsync();
    
        if (!string.IsNullOrEmpty(dto.ChainId))
        {
            queryable = queryable.Where(t => t.Metadata.ChainId == dto.ChainId);
        }
    
        if (dto.StartBlockHeight > 0)
        {
            queryable = queryable.Where(a => a.Metadata.Block.BlockHeight >= dto.StartBlockHeight);
        }
    
        if (dto.EndBlockHeight > 0)
        {
            queryable = queryable.Where(a => a.Metadata.Block.BlockHeight <= dto.EndBlockHeight);
        }
        
        var result = queryable
            .OrderBy(record => record.Metadata.Block.BlockHeight)
            .Skip(dto.SkipCount)
            .Take(dto.MaxResultCount)
            .ToList();
    
        return objectMapper.Map<List<LimitOrderIndex>, List<LimitOrderDto>>(result);
    }
    

    [Name("activeAddress")]
    public static async Task<ActiveAddressDto> ActiveAddressAsync(
        [FromServices] IReadOnlyRepository<LiquidityRecordIndex> liquidityRepository,
        [FromServices] IReadOnlyRepository<SwapRecordIndex> swapRepository,
        [FromServices] IReadOnlyRepository<LimitOrderIndex> limitOrderRepository,
        [FromServices] IObjectMapper objectMapper,
        GetTimeRangeDto dto
    )
    {
        var beforeAddresses = new HashSet<string>();
        var activeAddresses = new HashSet<string>();
        
        var liquidityBeforeQueryable = await liquidityRepository.GetQueryableAsync();
        liquidityBeforeQueryable = liquidityBeforeQueryable.Where(t => t.Timestamp <= dto.TimestampMin);
        var beforeLiquidityAddress = liquidityBeforeQueryable.AsEnumerable().GroupBy(item => item.Address)
            .Select(group => group.Key)
            .ToList();
        beforeLiquidityAddress.ForEach(str => beforeAddresses.Add(str));
        
        var liquidityQueryable = await liquidityRepository.GetQueryableAsync();
        liquidityQueryable = liquidityQueryable.Where(t => t.Timestamp > dto.TimestampMin);
        liquidityQueryable = liquidityQueryable.Where(t => t.Timestamp <= dto.TimestampMax);
        var liquidityAddress = liquidityQueryable.AsEnumerable().GroupBy(item => item.Address)
            .Select(group => group.Key)
            .ToList();
        liquidityAddress.ForEach(str => activeAddresses.Add(str));
        
        var swapBeforeQueryable = await swapRepository.GetQueryableAsync();
        swapBeforeQueryable = swapBeforeQueryable.Where(t => t.Timestamp <= dto.TimestampMin);
        var beforeSwapAddress = swapBeforeQueryable.AsEnumerable().GroupBy(item => item.Sender)
            .Select(group => group.Key)
            .ToList();
        beforeSwapAddress.ForEach(str => beforeAddresses.Add(str));
        
        var swapQueryable = await swapRepository.GetQueryableAsync();
        swapQueryable = swapQueryable.Where(t => t.Timestamp > dto.TimestampMin);
        swapQueryable = swapQueryable.Where(t => t.Timestamp <= dto.TimestampMax);
        var swapAddress = swapQueryable.AsEnumerable().GroupBy(item => item.Sender)
            .Select(group => group.Key)
            .ToList();
        swapAddress.ForEach(str => activeAddresses.Add(str));

        var limitOrderBeforeQueryable = await limitOrderRepository.GetQueryableAsync();
        limitOrderBeforeQueryable = limitOrderBeforeQueryable.Where(t => t.CommitTime <= dto.TimestampMin);
        var beforeLimitOrderAddress = limitOrderBeforeQueryable.AsEnumerable().GroupBy(item => item.Maker)
            .Select(group => group.Key)
            .ToList();
        beforeLimitOrderAddress.ForEach(str => beforeAddresses.Add(str));
        
        var limitOrderQueryable = await limitOrderRepository.GetQueryableAsync();
        limitOrderQueryable = limitOrderQueryable.Where(t => t.CommitTime > dto.TimestampMin);
        limitOrderQueryable = limitOrderQueryable.Where(t => t.CommitTime <= dto.TimestampMax);
        var limitOrderAddress = limitOrderQueryable.AsEnumerable().GroupBy(item => item.Maker)
            .Select(group => group.Key)
            .ToList();
        limitOrderAddress.ForEach(str => activeAddresses.Add(str));

        var newActiveAddress = new HashSet<string>();
        foreach (var activeAddress in activeAddresses)
        {
            if (!beforeAddresses.Contains(activeAddress))
            {
                newActiveAddress.Add(activeAddress);
            }
        }

        return new ActiveAddressDto()
        {
            ActiveAddresses = activeAddresses.ToList(),
            NewActiveAddresses = newActiveAddress.ToList(),
            ActiveAddressCount = activeAddresses.Count,
            NewActiveAddressCount = newActiveAddress.Count
        };
    }
    
    
    [Name("transactionVolume")]
    public static async Task<TransactionVolumeDto> TransactionVolumeAsync(
        [FromServices] IReadOnlyRepository<LiquidityRecordIndex> liquidityRepository,
        [FromServices] IReadOnlyRepository<SwapRecordIndex> swapRepository,
        [FromServices] IObjectMapper objectMapper,
        GetTimeRangeDto dto
    )
    {
        var result = new TransactionVolumeDto();
        var tokenFeeMap = new Dictionary<string, double>();
        
        var swapRecordQueryable = await swapRepository.GetQueryableAsync();
        swapRecordQueryable = swapRecordQueryable.Where(a => a.Timestamp > dto.TimestampMin);
        swapRecordQueryable = swapRecordQueryable.Where(a => a.Timestamp <= dto.TimestampMax);
        
        var swapRecordIndices = await GetAllSwapRecords(swapRecordQueryable);
        
        // include swap with pool & swap with limit order
        foreach (var swapRecordIndex in swapRecordIndices)
        {
            result.TransactionCount++;
            if (!tokenFeeMap.ContainsKey(swapRecordIndex.SymbolOut))
            {
                tokenFeeMap.Add(swapRecordIndex.SymbolOut, 0);
            }
            tokenFeeMap[swapRecordIndex.SymbolOut] += swapRecordIndex.AmountOut;
            
            foreach (var swapRecord in swapRecordIndex.SwapRecords)
            {
                result.TransactionCount++;
                if (!tokenFeeMap.ContainsKey(swapRecord.SymbolOut))
                {
                    tokenFeeMap.Add(swapRecord.SymbolOut, 0);
                }
                tokenFeeMap[swapRecord.SymbolOut] += swapRecord.AmountOut;
            }
        }

        var liquidityRecordQueryable = await liquidityRepository.GetQueryableAsync();
        liquidityRecordQueryable = liquidityRecordQueryable.Where(a => a.Timestamp > dto.TimestampMin);
        liquidityRecordQueryable = liquidityRecordQueryable.Where(a => a.Timestamp <= dto.TimestampMax);

        var liquidityRecordIndices = liquidityRecordQueryable.Skip(0).Take(10000).ToList();
        
        foreach (var liquidityRecordIndex in liquidityRecordIndices)
        {
            result.TransactionCount++;
            if (!tokenFeeMap.ContainsKey(liquidityRecordIndex.Token0))
            {
                tokenFeeMap.Add(liquidityRecordIndex.Token0, 0);
            }
            tokenFeeMap[liquidityRecordIndex.Token0] += liquidityRecordIndex.Token0Amount;
            if (!tokenFeeMap.ContainsKey(liquidityRecordIndex.Token1))
            {
                tokenFeeMap.Add(liquidityRecordIndex.Token1, 0);
            }
            tokenFeeMap[liquidityRecordIndex.Token1] += liquidityRecordIndex.Token1Amount;
        }
        
        foreach (var kv in tokenFeeMap)
        {
            result.TransactionVolumes.Add(new TokenAmountDto()
            {
                TokenSymbol = kv.Key,
                Amount = kv.Value
            });
        }

        return result;
    }
}