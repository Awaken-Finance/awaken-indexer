namespace SwapIndexer.GraphQL.Dto;

public class ActiveAddressDto
{
    public List<string> ActiveAddresses { get; set; } = new();
    public List<string> NewActiveAddresses { get; set; } = new();
    public int ActiveAddressCount { get; set; }
    public int NewActiveAddressCount { get; set; }
}