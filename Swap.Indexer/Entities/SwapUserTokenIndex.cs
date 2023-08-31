using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Swap.Indexer.Entities;

public class SwapUserTokenIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword]
    public string Address { get; set; }

    [Keyword]
    public string Symbol { get; set; }
    
    public long Balance { get; set; }
}