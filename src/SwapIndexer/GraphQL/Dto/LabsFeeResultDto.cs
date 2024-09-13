namespace SwapIndexer.GraphQL.Dto;

public class LabsFeeResultDto
{
    public List<TokenLabsFeeDto> Tokens { get; set; } = new();
}

public class TokenLabsFeeDto
{
    public string TokenSymbol { get; set; }
    public double LabsFee { get; set; }
}