namespace SwapIndexer.GraphQL;

public class GetLimitOrderRemainingUnfilledDto
{
    public string ChainId { get; set; }
    public string MakerAddress { get; set; }
    public string TokenSymbol { get; set; }
}