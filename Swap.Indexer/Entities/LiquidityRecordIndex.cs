using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Swap.Indexer.Entities;

public class LiquidityRecordIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public string Pair { get; set; }
    [Keyword] public string To { get; set; }
    [Keyword] public string Address { get; set; }
    public long Token0Amount { get; set; }
    public long Token1Amount { get; set; }
    [Keyword] public string Token0 { get; set; }
    [Keyword] public string Token1 { get; set; }
    public long LpTokenAmount { get; set; }
    public long Timestamp { get; set; }
    [Keyword] public string TransactionHash { get; set; }
    [Keyword] public string Channel { get; set; }
    [Keyword] public string Sender { get; set; }
    public LiquidityType Type { get; set; }

    public enum LiquidityType
    {
        Mint,
        Burn
    }
}