using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Swap.Indexer.Entities;

public class SwapRecordIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public string PairAddress { get; set; }
    [Keyword] public string Sender { get; set; }
    [Keyword] public string TransactionHash { get; set; }
    public long Timestamp { get; set; }
    public long AmountOut { get; set; }
    public long AmountIn { get; set; }
    public long TotalFee { get; set; }
    [Keyword] public string SymbolOut { get; set; }
    [Keyword] public string SymbolIn { get; set; }
    [Keyword] public string Channel { get; set; }
    public List<SwapRecord> SwapRecords { get; set; } = new();
    [Keyword] public string MethodName { get; set; }
    [Keyword] public string InputArgs { get; set; }
    public bool IsLimitOrder { get; set; }
    public long LabsFee { get; set; }
}

public class SwapRecord
{
    [Keyword] public string PairAddress { get; set; }
    public long AmountOut { get; set; }
    public long AmountIn { get; set; }
    public long TotalFee { get; set; }
    [Keyword] public string SymbolOut { get; set; }
    [Keyword] public string SymbolIn { get; set; }
    [Keyword] public string Channel { get; set; }
    public bool IsLimitOrder { get; set; }
}