namespace SwapIndexer.GraphQL;

public class GetPairTradeValueDto : GetTimeRangeDto
{
    public List<string> TradePairs { get; set; }
}