using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElf.CSharp.Core;
using SwapIndexer;
using SwapIndexer.Entities;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public abstract class LiquidityProcessorBase<TEvent> : LogEventProcessorBase<TEvent> where TEvent : IEvent<TEvent>, new()
{
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();
    
    public override string GetContractAddress(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.SwapContractAddressTDVVLevel1,
            AwakenSwapConst.tDVW => AwakenSwapConst.SwapContractAddressTDVWLevel1,
            _ => string.Empty
        };
    }

    protected async Task HandlerUserLiquidityAsync(LiquidityRecordIndex recordIndex, LogEventContext context)
    {
        var userLiquidityId = IdGenerateHelper.GetId(context.ChainId, recordIndex.Address, recordIndex.Pair);
        var userLiquidity = await GetEntityAsync<UserLiquidityIndex>(userLiquidityId);
        
        if (userLiquidity == null)
        {
            userLiquidity = new UserLiquidityIndex();
            ObjectMapper.Map(recordIndex, userLiquidity);
            userLiquidity.Id = userLiquidityId;
        }
        else
        {
            if (recordIndex.Type == LiquidityRecordIndex.LiquidityType.Mint)
            {
                userLiquidity.LpTokenAmount += recordIndex.LpTokenAmount;
            }
            else
            {
                userLiquidity.LpTokenAmount -= recordIndex.LpTokenAmount;
            }
        }
        await SaveEntityAsync(userLiquidity);

    }
}