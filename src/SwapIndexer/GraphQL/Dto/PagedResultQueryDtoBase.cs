namespace SwapIndexer.GraphQL;

public class PagedResultQueryDtoBase
{
    private const int MaxMaxResultCount = 10000;
    
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = 10000;
    
    public void Validate()
    {
        if (MaxResultCount > MaxMaxResultCount)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxResultCount),
                $"Max allowed value for {nameof(MaxResultCount)} is {MaxMaxResultCount}.");
        }
    }
}