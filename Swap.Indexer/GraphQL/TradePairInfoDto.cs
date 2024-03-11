using Nest;

namespace Swap.Indexer.GraphQL;

public class TradePairInfoDtoPageResultDto
{
    public long TotalCount { get; set; }
    public List<TradePairInfoDto> Data { get; set; }
}

public class TradePairInfoDto
{
    [Keyword] 
    public string Id { get; set; }
    [Keyword]
    public string ChainId { get; set; }
    [Keyword] 
    public string Address { get; set; }
    [Keyword] 
    public string Token0Symbol { get; set; }
    [Keyword] 
    public string Token1Symbol { get; set; }
    
    public double FeeRate { get; set; }
    
    public bool IsTokenReversed { get; set; }
    
    public long BlockHeight { get; set; }
}