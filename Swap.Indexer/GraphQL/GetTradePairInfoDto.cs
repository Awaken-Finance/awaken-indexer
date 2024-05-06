namespace Swap.Indexer.GraphQL;

public class GetTradePairInfoDto : PagedResultQueryDtoBase
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    public string Token0Symbol { get; set; }
    public double FeeRate { get; set; }
    public string Token1Symbol { get; set; }
    public string TokenSymbol { get; set; }
    public string Address { get; set; }
    public long StartBlockHeight { get; set; }
    public long EndBlockHeight { get; set; }
}