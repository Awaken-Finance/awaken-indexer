
using AeFinder.Sdk.Entities;
using Nest;

namespace SwapIndexer.Entities;

public class TradePairInfoIndex : AeFinderEntity, IAeFinderEntity
{
    [Keyword] 
    public override string Id { get; set; }
    [Keyword] 
    public string Address { get; set; }
    [Keyword] 
    public string Token0Symbol { get; set; }
    [Keyword] 
    public string Token1Symbol { get; set; }
    [Keyword] 
    public string TransactionHash { get; set; }
    public double FeeRate { get; set; }
    public bool IsTokenReversed { get; set; }
}