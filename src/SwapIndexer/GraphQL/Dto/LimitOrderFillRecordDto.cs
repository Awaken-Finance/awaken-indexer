namespace SwapIndexer.GraphQL.Dto;

public class LimitOrderFillRecordDto
{
    public string ChainId { get; set; }
    public long OrderId { get; set; }
    public string MakerAddress { get; set; }
    public string SymbolIn { get; set; }
    public string SymbolOut { get; set; }
    public string TakerAddress { get; set; }
    public string TransactionHash { get; set; }
    public long AmountInFilled { get; set; }
    public long AmountOutFilled { get; set; }
    public long TotalFee { get; set; }
    public long TransactionTime { get; set; }
    public long TransactionFee { get; set; }
}