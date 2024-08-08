using Awaken.Contracts.Order;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class LimitOrderRemovedProcessor : LimitOrderProcessorBase<LimitOrderRemoved>
{
    public LimitOrderRemovedProcessor(
        ILogger<LimitOrderRemovedProcessor> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger,objectMapper, contractInfoOptions, repository)
    {
    }
    
    protected override async Task HandleEventAsync(LimitOrderRemoved eventValue, LogEventContext context)
    {
        Logger.Info("received LimitOrderRemoved:" + eventValue + ",context:" + context);
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.OrderId);
        var recordIndex = await Repository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (recordIndex == null)
        {
            Logger.Info("received error LimitOrderRemoved:" + eventValue + ",can not find order id:" + id);
            return;
        }

        recordIndex.RemoveTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.RemoveTime.ToDateTime());
        recordIndex.LastUpdateTime = recordIndex.RemoveTime;
        recordIndex.LimitOrderStatus = eventValue.ReasonType == ReasonType.Expired
            ? LimitOrderStatus.Epired
            : LimitOrderStatus.Revoked;
        
        recordIndex.FillRecords.Add(new FillRecord()
        {
            TransactionTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.RemoveTime.ToDateTime()),
            TransactionHash = context.TransactionId,
            Status = recordIndex.LimitOrderStatus
        });
        ObjectMapper.Map(context, recordIndex);
        Logger.Info("LimitOrderIndex:" + recordIndex);
        await Repository.AddOrUpdateAsync(recordIndex);
    }
}
