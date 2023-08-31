namespace Swap.Indexer.GraphQL;

public class GetTradePairInfoDto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    
    public string Token0Symbol { get; set; }
    
    public double FeeRate { get; set; }
    public string Token1Symbol { get; set; }
    
    public string TokenSymbol { get; set; }
    
    public string Address { get; set; }
    
    public int SkipCount { get; set; }
    
    public int MaxResultCount { get; set; }
}