using SwapIndexer.Entities;

namespace SwapIndexer.GraphQL;

public class GetLiquidityRecordDto : PagedResultQueryDtoBase
{
    public string? ChainId { get; set; }
    public string? Address { get; set; }
    public string? Pair { get; set; }
    public LiquidityRecordIndex.LiquidityType? Type { get; set; }
    public string? TokenSymbol { get; set; }
    public string? TransactionHash { get; set; }
    public string? Token0 { get; set; }
    public string? Token1 { get; set; }
    public long TimestampMin { get; set; }
    public long TimestampMax { get; set; }
    public string? Sorting { get; set; }
}