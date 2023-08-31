namespace Swap.Indexer.GraphQL;

public class GetUserLiquidityDto
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string Pair { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
    public string Sorting { get; set; }
}