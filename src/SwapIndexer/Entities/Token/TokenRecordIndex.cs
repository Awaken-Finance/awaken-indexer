
using AeFinder.Sdk.Entities;
using JetBrains.Annotations;
using Nest;

namespace SwapIndexer.Entities.Token;

public class TokenRecordIndex : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public override string Id { get; set; }

    [Keyword] [NotNull] public virtual string Address { get; set; }

    [Keyword] [NotNull] public virtual string Symbol { get; set; }

    public virtual int Decimals { get; set; }
    
}