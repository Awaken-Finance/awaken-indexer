using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using Google.Protobuf;
using SwapIndexer.Providers;
using SwapIndexer;
using Volo.Abp.ObjectMapping;

namespace SwapIndexer.Processors;

public abstract class LimitOrderProcessorBase<TEvent> : LogEventProcessorBase<TEvent> where TEvent : IEvent<TEvent>, new()
{
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();
    
    protected readonly IAElfDataProvider AElfDataProvider;
    protected LimitOrderProcessorBase(
        IAElfDataProvider aelfDataProvider)
    {
        AElfDataProvider = aelfDataProvider;
    }

    public override string GetContractAddress(string chainId)
    {
        return chainId switch
        {
            AwakenSwapConst.tDVV => AwakenSwapConst.LimitOrderContractAddressTDVV,
            AwakenSwapConst.tDVW => AwakenSwapConst.LimitOrderContractAddressTDVW,
            _ => string.Empty
        };
    }
    
    public async Task<long> GetTransactionFeeAsync(Transaction transaction)
    {
        try
        {
            Logger.LogDebug($"Get txn fee, txn id: {transaction.TransactionId}, log event count: {transaction.LogEvents.Count()}");
            foreach (var logEvent in transaction.LogEvents)
            {
                Logger.LogDebug($"Get txn fee, txn id: {transaction.TransactionId}, event: {logEvent.EventName}");
                foreach (var extraProperty in logEvent.ExtraProperties)
                {
                    Logger.LogDebug($"Get txn fee, txn id: {transaction.TransactionId}, event: {logEvent.EventName}, extraProperty key: {extraProperty.Key}, extraProperty value: {extraProperty.Value}");
                }
            }
            var extraProperties = transaction.LogEvents?.FirstOrDefault(l => l.EventName == nameof(TransactionFeeCharged))
                ?.ExtraProperties;
            if (extraProperties == null || !extraProperties.ContainsKey("NonIndexed"))
            {
                return 0;
            }
            var transactionFeeCharged = TransactionFeeCharged.Parser.ParseFrom(
                ByteString.FromBase64(extraProperties["NonIndexed"]));
            return transactionFeeCharged.Amount;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "GetTransactionFeeAsync failed.");
        }
        return 0;
    }
}