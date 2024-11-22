using JetBrains.Annotations;
using Nest;

namespace SwapIndexer.Application.Contracts.Token;

public class TokenRecordIndexDto
{
    [Keyword] public string Id { get; set; }

    [Keyword] [NotNull] public virtual string Address { get; set; }

    [Keyword] [NotNull] public virtual string Symbol { get; set; }

    public virtual int Decimals { get; set; }
}