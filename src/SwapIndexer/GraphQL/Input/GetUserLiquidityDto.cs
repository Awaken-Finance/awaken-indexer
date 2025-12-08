namespace SwapIndexer.GraphQL;

public class GetUserLiquidityDto : PagedResultQueryDtoBase
{
    public string? ChainId { get; set; }
    public string? Address { get; set; }
    public string? Pair { get; set; }
    public string? Sorting { get; set; }
}