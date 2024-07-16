namespace Swap.Indexer.Options;

public class ContractInfoOptions
{
    public List<ContractInfo> ContractInfos { get; set; }
}

public class ContractInfo
{
    public string ChainId { get; set; }
    public string SwapContractAddress { get; set; }
    public string MultiTokenContractAddress { get; set; }
    
    public string HooksContractAddress { get; set; }
    
    public string CaContractAddress { get; set; }
    public double FeeRate { get; set; }
    public int Level { get; set; }
}