using AeFinder.Sdk.Entities;
using Nest;

namespace SwapIndexer.Entities;

public class LimitOrderFillRecordIndex : AeFinderEntity, IAeFinderEntity
{
    public long OrderId { get; set; }
    [Keyword] public string MakerAddress { get; set; }
    [Keyword] public string SymbolIn { get; set; }
    [Keyword] public string SymbolOut { get; set; }
    [Keyword] public string TakerAddress { get; set; }
    [Keyword] public string TransactionHash { get; set; }
    public long AmountInFilled { get; set; }
    public long AmountOutFilled { get; set; }
    public long TotalFee { get; set; }
    public long TransactionTime { get; set; }
    public long TransactionFee { get; set; }
}