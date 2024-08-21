namespace SwapIndexer.GraphQL;

public class GetPairReserveDto
{
    public string? ChainId { get; set; }
    public string SymbolA { get; set; }
    public string SymbolB { get; set; }
}