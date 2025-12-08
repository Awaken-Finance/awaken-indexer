using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Awaken.Contracts.Hooks;
using SwapIndexer;
using SwapIndexer.Entities;
using SwapIndexer.Processors;

namespace Swap.Indexer.Processors;

public class LabsFeeChargedProcessor : SwapProcessorBase<LabsFeeCharged>
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

    public override async Task ProcessAsync(LabsFeeCharged eventValue, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, context.Transaction.TransactionId, context.Block.BlockHeight);
        var recordIndex = await GetEntityAsync<SwapRecordIndex>(id) ?? new SwapRecordIndex
        {
            Id = id
        };
        recordIndex.LabsFee = eventValue.Amount;
        recordIndex.LabsFeeSymbol = eventValue.Symbol;
        Logger.LogInformation($"LabsFeeCharged: txn id: {context.Transaction.TransactionId}, index labs fee: {recordIndex.LabsFee}");
        await SaveEntityAsync(recordIndex);

    }
}