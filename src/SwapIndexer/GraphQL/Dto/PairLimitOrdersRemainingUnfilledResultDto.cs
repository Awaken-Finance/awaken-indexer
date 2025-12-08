namespace SwapIndexer.GraphQL.Dto;

public class PairLimitOrdersRemainingUnfilledResultDto
{
    public string SymbolIn { get; set; }
    public string SymbolOut { get; set; }
    public int OrderCount { get; set; }
    public int PriceCount { get; set; }
    public List<long> PriceList { get; set; } = new();
}