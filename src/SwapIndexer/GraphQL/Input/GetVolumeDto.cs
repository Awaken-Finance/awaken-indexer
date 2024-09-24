namespace SwapIndexer.GraphQL;

public class GetTransactionVolumeDto : GetTimeRangeDto
{
    // 0:all, 1:swap, 2:liquidity
    public int? TransactionType { get; set; }
}

public enum TransactionType
{
    All = 0,
    Swap,
    Liquidity
}