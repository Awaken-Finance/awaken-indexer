using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Swap.Indexer.Entities;

public class SyncRecordIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public string PairAddress { get; set; }
    [Keyword] public string SymbolA { get; set; }
    [Keyword] public string SymbolB { get; set; }
    public long ReserveA { get; set; }
    public long ReserveB { get; set; }
    public long Timestamp { get; set; }
}