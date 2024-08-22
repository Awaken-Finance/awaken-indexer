using Awaken.Contracts.Order;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Hooks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Swap.Indexer.Providers;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class LimitOrderTotalFilledProcessor : LimitOrderProcessorBase<LimitOrderTotalFilled>
{
    private readonly IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> _swapRecordIndexRepository;

    public LimitOrderTotalFilledProcessor(
        ILogger<LimitOrderTotalFilledProcessor> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<LimitOrderIndex, LogEventInfo> repository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfDataProvider aelfDataProvider,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository) : base(logger,objectMapper, contractInfoOptions, repository, aelfDataProvider)
    {
        _swapRecordIndexRepository = swapRecordIndexRepository;
    }
    
    protected override async Task HandleEventAsync(LimitOrderTotalFilled eventValue, LogEventContext context)
    {
        Logger.Info("received LimitOrderTotalFilled:" + eventValue + ",context:" + context);
        var indexId = IdGenerateHelper.GetId(context.ChainId, context.TransactionId, context.BlockHeight);
        var record = await _swapRecordIndexRepository.GetFromBlockStateSetAsync(indexId, context.ChainId);
        var swapSymbolIn = eventValue.SymbolOut;
        var swapSymbolOut = eventValue.SymbolIn;
        var swapAmountIn = eventValue.AmountOutFilled;
        var swapAmountOut = eventValue.AmountInFilled;
        if (record == null || record.TransactionHash.IsNullOrWhiteSpace())
        {
            record ??= new SwapRecordIndex
            {
                Id = indexId,
                Sender = eventValue.Sender.ToBase58()
            };
            record.ChainId = context.ChainId;
            record.TransactionHash = context.TransactionId;
            record.Timestamp = DateTimeHelper.ToUnixTimeMilliseconds(context.BlockTime);
            record.AmountOut = swapAmountOut;
            record.AmountIn = swapAmountIn;
            record.SymbolIn = swapSymbolIn;
            record.SymbolOut = swapSymbolOut;
            record.IsLimitOrder = true;
        }
        else
        {
            var swapRecord = new SwapRecord
            {
                AmountIn = swapAmountIn,
                AmountOut = swapAmountOut,
                SymbolIn = swapSymbolIn,
                SymbolOut = swapSymbolOut,
                IsLimitOrder = true
            };
            record.SwapRecords.Add(swapRecord);
        }
        ObjectMapper.Map(context, record);
        await _swapRecordIndexRepository.AddOrUpdateAsync(record);
    }
}
