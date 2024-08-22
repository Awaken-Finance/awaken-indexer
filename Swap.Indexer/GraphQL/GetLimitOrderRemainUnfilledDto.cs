namespace Swap.Indexer.GraphQL;

public class GetLimitOrderRemainingUnfilledDto
{
    public string ChainId { get; set; }
    public string MakerAddress { get; set; }
    public string TokenSymbol { get; set; }
    
    public void Validate()
    {
        if (string.IsNullOrEmpty(MakerAddress))
        {
            throw new Exception("MakerAddress is required.");
        }
        if (string.IsNullOrEmpty(ChainId))
        {
            throw new Exception("ChainId is required.");
        }
        if (string.IsNullOrEmpty(TokenSymbol))
        {
            throw new Exception("TokenSymbol is required.");
        }
    }
}