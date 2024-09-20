namespace SwapIndexer.GraphQL.Dto;

public class TransactionVolumeDto
{
    public List<TokenAmountDto> TransactionVolumes { get; set; } = new();
    public long TransactionCount { get; set; }
}

public class TokenAmountDto
{
    public string TokenSymbol { get; set; }
    public double Amount { get; set; }
}