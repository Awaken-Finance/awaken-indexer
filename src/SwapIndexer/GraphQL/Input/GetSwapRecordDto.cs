namespace SwapIndexer.GraphQL;

public class GetSwapRecordDto : PagedResultQueryDtoBase
{
    public string? ChainId { get; set; }
    public string? PairAddress { get; set; }
    public string? Sender { get; set; }
    public string? TransactionHash { get; set; }
    public long? Timestamp { get; set; }
    public long? AmountOut { get; set; }
    public long? AmountIn { get; set; }
    public string? SymbolOut { get; set; }
    public string? SymbolIn { get; set; }
    public string? Channel { get; set; }
}