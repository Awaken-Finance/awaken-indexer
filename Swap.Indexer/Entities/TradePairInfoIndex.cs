using AElf.Indexing.Elasticsearch;
using Nest;

namespace Swap.Indexer.Entities;

public class TradePairInfoIndex : SwapIndexerEntity<string>, IIndexBuild
{
    [Keyword] 
    public override string Id { get; set; }
    [Keyword] 
    public string Address { get; set; }
    [Keyword] 
    public string Token0Symbol { get; set; }
    [Keyword] 
    public string Token1Symbol { get; set; }
    [Keyword] 
    public string TransactionHash { get; set; }
    public double FeeRate { get; set; }
    public bool IsTokenReversed { get; set; }
}