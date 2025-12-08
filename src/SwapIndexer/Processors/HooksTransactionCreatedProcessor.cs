using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Hooks;
using Newtonsoft.Json;
using SwapIndexer;
using SwapIndexer.Entities;

namespace SwapIndexer.Processors;

public class HooksTransactionCreatedProcessor : SwapProcessorBase<HooksTransactionCreated>
{
    public override string GetContractAddress(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.HooksContractAddressTDVV,
            AwakenSwapConst.tDVW => AwakenSwapConst.HooksContractAddressTDVW,
            _ => string.Empty
        };
    }

    public override async Task ProcessAsync(HooksTransactionCreated eventValue, LogEventContext context)
    {
        if (eventValue.MethodName == "SwapExactTokensForTokens" || eventValue.MethodName == "SwapTokensForExactTokens")
        {
            var id = IdGenerateHelper.GetId(context.ChainId, context.Transaction.TransactionId, context.Block.BlockHeight);
            
            Logger.LogInformation($"HooksTransactionCreated begin, " +
                                  $"txnId: {context.Transaction.TransactionId}, " +
                                  $"MethodName: {eventValue.MethodName}, " +
                                  $"InputArgs: {eventValue.Args}, " +
                                  $"Id: {id}");
            
            var recordIndex = await GetEntityAsync<SwapRecordIndex>(id) ?? new SwapRecordIndex
            {
                Id = id
            };

            recordIndex.Sender = eventValue.Sender.ToBase58();
            recordIndex.MethodName = eventValue.MethodName;
            recordIndex.InputArgs = eventValue.Args.ToBase64();
            
            await SaveEntityAsync(recordIndex);
        }
    }
}