using AeFinder.Sdk.Processor;
using AElf.CSharp.Core;
using SwapIndexer;

using Microsoft.Extensions.Options;
using SwapIndexer.Entities;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public abstract class SyncProcessorBase<TEvent> : LogEventProcessorBase<TEvent> where TEvent : IEvent<TEvent>, new()
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
}