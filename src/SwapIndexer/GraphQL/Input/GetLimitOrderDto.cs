using SwapIndexer.Entities;

namespace SwapIndexer.GraphQL;

public class GetLimitOrderDto : PagedResultQueryDtoBase
{
    public string? MakerAddress { get; set; }
    public int? LimitOrderStatus { get; set; }
    public string? TokenSymbol { get; set; }
    public string? Sorting { get; set; }
}

public class GetLimitOrderDetailDto
{
    public long OrderId { get; set; }
}