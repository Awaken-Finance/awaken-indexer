namespace SwapIndexer.GraphQL.Dto;

public class PairTransactionVolumeDto
{
    public List<PairTransactionVolume> PairTransactionVolumes { get; set; } = new();
}

public class PairTransactionVolume
{
    public string PairAddress { get; set; }
    public TransactionVolumeDto TokenTransactionVolume { get; set; }
}