using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Hooks;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Swap.Indexer.Entities;
using Swap.Indexer.Options;
using Volo.Abp.ObjectMapping;

namespace Swap.Indexer.Processors;

public class HooksTransactionCreatedProcessor : SwapProcessorBase<HooksTransactionCreated>
{
    public HooksTransactionCreatedProcessor(ILogger<SwapProcessorBase<HooksTransactionCreated>> logger, IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository,
        IAElfIndexerClientEntityRepository<LiquidityRecordIndex, LogEventInfo> liquidityRecordRepository
        ) 
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository)
    {
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(o => o.ChainId == chainId && o.Level == 1).HooksContractAddress;
    }

    protected override async Task HandleEventAsync(HooksTransactionCreated eventValue, LogEventContext context)
    {
        if (eventValue.MethodName == "SwapExactTokensForTokens" || eventValue.MethodName == "SwapTokensForExactTokens")
        {
            Logger.Info("received HooksTransactionCreated:" + eventValue + ",txn id:" + context.TransactionId);
            var id = IdGenerateHelper.GetId(context.ChainId, context.TransactionId, context.BlockHeight);
            var recordIndex = await SwapRecordIndexRepository.GetFromBlockStateSetAsync(id, context.ChainId) ?? new SwapRecordIndex
            {
                Id = id
            };

            ObjectMapper.Map(context, recordIndex);
            recordIndex.Sender = eventValue.Sender.ToBase58();
            recordIndex.MethodName = eventValue.MethodName;
            recordIndex.InputArgs = eventValue.Args.ToBase64();
            Logger.Info("HooksTransactionCreated:" + recordIndex);
            await SwapRecordIndexRepository.AddOrUpdateAsync(recordIndex);
        }
    }
}