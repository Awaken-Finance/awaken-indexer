using SwapIndexer.Entities;

namespace SwapIndexer.GraphQL;

public class LiquidityRecordPageResultDto
{
    public long TotalCount { get; set; }
    public List<LiquidityRecordDto> Data { get; set; }
}

public class LiquidityRecordDto
{
    public string ChainId { get; set; }
    public string Pair { get; set; }
    public string To { get; set; }
    public string Address { get; set; }
    public long Token0Amount { get; set; }
    public long Token1Amount { get; set; }
    public string Token0 { get; set; }
    public string Token1 { get; set; }
    public long LpTokenAmount { get; set; }
    public long Timestamp { get; set; }
    public string TransactionHash { get; set; }
    public string? Channel { get; set; }
    public string Sender { get; set; }
    public LiquidityRecordIndex.LiquidityType Type { get; set; }
    public long BlockHeight { get; set; }
}