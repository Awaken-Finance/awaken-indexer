namespace SwapIndexer.GraphQL;

public class GetUserTokenDto
{
    public string? ChainId { get; set; }
    public string? Address { get; set; }
    public string? Symbol { get; set; }
}