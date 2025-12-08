namespace SwapIndexer.GraphQL;

public class UserLiquidityPageResultDto
{
    public long TotalCount { get; set; }
    public List<UserLiquidityDto> Data { get; set; }
}

public class UserLiquidityDto
{
    public string ChainId { get; set; }
    public string Pair { get; set; }
    public string Address { get; set; }
    public long LpTokenAmount { get; set; }
    public long Timestamp { get; set; }
}