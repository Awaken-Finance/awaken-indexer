using System;
using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Swap.Indexer.Entities
{
    public class UserLiquidityIndex : AElfIndexerClientEntity<string>, IIndexBuild
    {
        [Keyword]
        public string Pair { get; set; }
        [Keyword]
        public string Address { get; set; }
        public long LpTokenAmount { get; set; }
        public long Timestamp { get; set; }
    }
}