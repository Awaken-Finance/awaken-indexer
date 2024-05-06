namespace Swap.Indexer.GraphQL;

public class GetPullLiquidityRecordDto : PagedResultQueryDtoBase
{
    public string ChainId { get; set; }
    public long StartBlockHeight { get; set; }
    public long EndBlockHeight { get; set; }
}