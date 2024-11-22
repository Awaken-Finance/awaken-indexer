
using AeFinder.Sdk.Entities;
using Nest;

namespace SwapIndexer.Entities;

public class SyncRecordIndex : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string PairAddress { get; set; }
    [Keyword] public string SymbolA { get; set; }
    [Keyword] public string SymbolB { get; set; }
    [Keyword] public string TransactionHash { get; set; }
    public long ReserveA { get; set; }
    public long ReserveB { get; set; }
    public long Timestamp { get; set; }
}