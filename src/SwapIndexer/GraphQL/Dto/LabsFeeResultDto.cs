namespace SwapIndexer.GraphQL.Dto;

public class LabsFeeResultDto
{
    public List<TokenLabsFeeDto> tokens { get; set; } = new();
}

public class TokenLabsFeeDto
{
    public string tokenSymbol { get; set; }
    public long labsFee { get; set; }
}