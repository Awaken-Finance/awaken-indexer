using Swap.Indexer.Entities;

namespace Swap.Indexer.GraphQL;

public class GetLimitOrderDto : PagedResultQueryDtoBase
{
    public string MakerAddress { get; set; }
    public LimitOrderStatus LimitOrderStatus { get; set; }
    public long OrderId { get; set; }
    public string Sorting { get; set; }
}