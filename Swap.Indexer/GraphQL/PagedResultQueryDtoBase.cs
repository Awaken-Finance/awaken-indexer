namespace Swap.Indexer.GraphQL;

public class PagedResultQueryDtoBase
{
    private const int MaxMaxResultCount = 1000;
    
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = 10;
    
    public void Validate()
    {
        if (MaxResultCount > MaxMaxResultCount)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxResultCount),
                $"Max allowed value for {nameof(MaxResultCount)} is {MaxMaxResultCount}.");
        }
    }
}