namespace Swap.Indexer.Options;

public class FactoryContractOptions
{
    //contract address -> fee rate
    public Dictionary<string, double> Contracts { get; set; } = new();
}