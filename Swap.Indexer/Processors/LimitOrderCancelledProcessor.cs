using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Order;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class LimitOrderCancelledProcessor : LimitOrderProcessorBase<LimitOrderCancelled>
{
    public LimitOrderCancelledProcessor(
        ILogger<LimitOrderCancelledProcessor> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger,objectMapper, contractInfoOptions, repository)
    {
    }
    
    protected override async Task HandleEventAsync(LimitOrderCancelled eventValue, LogEventContext context)
    {
        Logger.Info("received LimitOrderCancelled:" + eventValue + ",context:" + context);
        var id = IdGenerateHelper.GetId(eventValue.OrderId);
        var recordIndex = await Repository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (recordIndex == null)
        {
            Logger.Info("received error LimitOrderCancelled:" + eventValue + ",can not find order id:" + id);
            return;
        }

        recordIndex.CancelTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.CancelTime.ToDateTime());
        recordIndex.LastUpdateTime = recordIndex.CancelTime;
        recordIndex.LimitOrderStatus = LimitOrderStatus.Cancelled;
        recordIndex.FillRecords.Add(new FillRecord()
        {
            TransactionTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.CancelTime.ToDateTime()),
            TransactionHash = context.TransactionId,
            Status = LimitOrderRecordStatus.Cancel
        });
        
        ObjectMapper.Map(context, recordIndex);
        
        Logger.Info("LimitOrderIndex:" + recordIndex);
        await Repository.AddOrUpdateAsync(recordIndex);
    }
}
