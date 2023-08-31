namespace Swap.Indexer.GraphQL;

public class GetChainBlockHeightDto
{
    public string ChainId { get; set; }
    public long StartBlockHeight { get; set; }
    public long EndBlockHeight { get; set; }
}