namespace SwapIndexer.GraphQL;

public class PairReserveDto
{
    public List<TradePairInfoDto> TradePairs { get; set; }
    public List<SyncRecordDto> SyncRecords { get; set; }
    public long TotalReserveA { get; set; }
    public long TotalReserveB { get; set; }
}