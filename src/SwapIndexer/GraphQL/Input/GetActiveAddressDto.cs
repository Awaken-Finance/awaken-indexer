namespace SwapIndexer.GraphQL;

public class GetActiveAddressDto : GetTimeRangeDto
{
    public int? TransactionType { get; set; }
}

public enum ActiveAddressTransactionType
{
    Swap = 1,
    AddLiquidity,
    RemoveLiquidity,
    Limit
}