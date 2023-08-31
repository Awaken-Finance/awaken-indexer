using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using JetBrains.Annotations;
using Nest;

namespace Swap.Indexer.Entities.Token;

public class TokenRecordIndex : AElfIndexerClientEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }

    [Keyword] [NotNull] public virtual string Address { get; set; }

    [Keyword] [NotNull] public virtual string Symbol { get; set; }

    public virtual int Decimals { get; set; }
    
}