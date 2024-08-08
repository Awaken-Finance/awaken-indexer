using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Swap.Indexer.Entities;

public class LimitOrderIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    public long OrderId { get; set; }
    [Keyword] public string Maker { get; set; }
    [Keyword] public string SymbolIn { get; set; }
    [Keyword] public string SymbolOut { get; set; }
    [Keyword] public string TransactionHash { get; set; }
    public long AmountIn { get; set; }
    public long AmountOut { get; set; }
    public long AmountInFilled { get; set; }
    public long AmountOutFilled { get; set; }
    public long Deadline { get; set; }
    public long CommitTime { get; set; }
    public long FillTime { get; set; }
    public long CancelTime { get; set; }
    public long RemoveTime { get; set; }
    public long LastUpdateTime { get; set; }
    public LimitOrderStatus LimitOrderStatus { get; set; }
    public List<FillRecord> FillRecords { get; set; } = new();
}


public class FillRecord
{
    public string TakerAddress { get; set; }
    public long AmountInFilled { get; set; }
    public long AmountOutFilled { get; set; }
    public long TransactionTime { get; set; }
    public string TransactionHash { get; set; }
    public LimitOrderStatus Status { get; set; }
}

public enum LimitOrderStatus
{
    Committed = 1,
    PartiallyFilling = 2,
    FullFilled = 3,
    Cancelled = 4,
    Epired = 5,
    Revoked = 6
}
