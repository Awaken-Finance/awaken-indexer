namespace SwapIndexer.GraphQL;

public class GetPairLimitOrdersRemainingUnfilledDto
{
    public string? ChainId { get; set; }
    public string? MakerAddress { get; set; }
    public string? SymbolIn { get; set; }
    public string? SymbolOut { get; set; }
}