using AElfIndexer;

namespace Swap.Indexer.GraphQL;

public class GetSyncStateDto
{
    public string ChainId { get; set; }
    public BlockFilterType FilterType { get; set; }
}