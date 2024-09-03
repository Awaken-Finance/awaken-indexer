namespace SwapIndexer.GraphQL;

public class GetSyncRecordDto : PagedResultQueryDtoBase
{
    public string? ChainId { get; set; }
    public string? PairAddress { get; set; }
    public string? SymbolA { get; set; }
    public string? SymbolB { get; set; }
    public long? ReserveA { get; set; }
    public long? ReserveB { get; set; }
    public long? Timestamp { get; set; }
    public long? TimestampMax { get; set; }
}