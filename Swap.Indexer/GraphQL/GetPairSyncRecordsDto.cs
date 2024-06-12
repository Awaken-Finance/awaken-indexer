namespace Swap.Indexer.GraphQL;

public class GetPairSyncRecordsDto
{
    public string ChainId { get; set; }
    public List<string> PairAddresses { get; set; }
}