using SwapIndexer.Entities;
using SwapIndexer.Entities;

namespace SwapIndexer.GraphQL;

public class LimitOrderPageResultDto
{
    public long TotalCount { get; set; }
    public List<LimitOrderDto> Data { get; set; }
}

public class LimitOrderDto
{
    public string ChainId { get; set; }
    public long OrderId { get; set; }
    public string Maker { get; set; }
    public string SymbolIn { get; set; }
    public string SymbolOut { get; set; }
    public string TransactionHash { get; set; }
    public long TransactionFee { get; set; }
    public long AmountIn { get; set; }
    public long AmountOut { get; set; }
    public long AmountInFilled { get; set; }
    public long AmountOutFilled { get; set; }
    public long Deadline { get; set; }
    public long CommitTime { get; set; }
    public long FillTime { get; set; }
    public long CancelTime { get; set; }
    public long RemoveTime { get; set; }
    public long LastUpdateTime { get; set; }
    public LimitOrderStatus LimitOrderStatus { get; set; }
    public List<FillRecord> FillRecords { get; set; }
}