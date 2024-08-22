using Awaken.Contracts.Order;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Swap.Indexer.Providers;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class LimitOrderFilledProcessor : LimitOrderProcessorBase<LimitOrderFilled>
{
    public LimitOrderFilledProcessor(
        ILogger<LimitOrderFilledProcessor> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfDataProvider aelfDataProvider) : base(logger,objectMapper, contractInfoOptions, repository, aelfDataProvider)
    {
    }
    
    protected override async Task HandleEventAsync(LimitOrderFilled eventValue, LogEventContext context)
    {
        Logger.Info("received LimitOrderFilled:" + eventValue + ",context:" + context);
        var id = IdGenerateHelper.GetId(context.ChainId, eventValue.OrderId);
        var recordIndex = await Repository.GetFromBlockStateSetAsync(id, context.ChainId);
        if (recordIndex == null)
        {
            Logger.Info("received error LimitOrderFilled:" + eventValue + ",can not find order id:" + id);
            return;
        }

        recordIndex.LimitOrderStatus = LimitOrderStatus.PartiallyFilling;
        recordIndex.FillTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.FillTime.ToDateTime());
        recordIndex.LastUpdateTime = recordIndex.FillTime;
        recordIndex.AmountInFilled += eventValue.AmountInFilled;
        recordIndex.AmountOutFilled += eventValue.AmountOutFilled;
        var transactionFee = 0L;
        var takerAddress = eventValue.Taker.ToBase58();
        if (takerAddress == recordIndex.Maker)
        {
            transactionFee = await AElfDataProvider.GetTransactionFeeAsync(context.ChainId, context.TransactionId);
        }
        recordIndex.FillRecords.Add(new FillRecord()
        {
            AmountInFilled = eventValue.AmountInFilled,
            AmountOutFilled = eventValue.AmountOutFilled,
            TransactionTime = DateTimeHelper.ToUnixTimeMilliseconds(eventValue.FillTime.ToDateTime()),
            TakerAddress = takerAddress,
            TransactionHash = context.TransactionId,
            Status = LimitOrderStatus.PartiallyFilling,
            TotalFee = eventValue.TotalFee,
            TransactionFee = transactionFee
        });
        
        if ((recordIndex.AmountIn > 0 && recordIndex.AmountIn == recordIndex.AmountInFilled) || 
            (recordIndex.AmountOut > 0 && recordIndex.AmountOut == recordIndex.AmountOutFilled))
        {
            recordIndex.LimitOrderStatus = LimitOrderStatus.FullFilled;
        }
        
        ObjectMapper.Map(context, recordIndex);
        Logger.Info("LimitOrderIndex:" + recordIndex);
        await Repository.AddOrUpdateAsync(recordIndex);
    }
}
