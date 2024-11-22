
using AeFinder.Sdk.Entities;
using Nest;

namespace SwapIndexer.Entities;

public class SwapUserTokenIndex : AeFinderEntity, IAeFinderEntity
{
    [Keyword]
    public string Address { get; set; }

    [Keyword]
    public string Symbol { get; set; }
    
    public long Balance { get; set; }
    [Keyword]
    public string ImageUri { get; set; }
}