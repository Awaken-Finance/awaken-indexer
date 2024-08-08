namespace Swap.Indexer.GraphQL;

public class UserTokenDto : GetUserTokenDto
{
    public long Balance { get; set; }
    public string ImageUri { get; set; }
}