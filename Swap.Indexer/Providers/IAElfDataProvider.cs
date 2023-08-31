using AElf;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AElfIndexer.Client.Providers;
using Google.Protobuf;

namespace Swap.Indexer.Providers;

public interface IAElfDataProvider
{
    Task<long> GetBalanceAsync(string chainId, string symbol, Address owner);
}

public class AElfDataProvider : IAElfDataProvider
{
    private const string PrivateKey = "09da44778f8db2e602fb484334f37df19e221c84c4582ce5b7770ccfbc3ddbef";
    private readonly IAElfClientProvider _aElfClientProvider;

    public AElfDataProvider(IAElfClientProvider aElfClientProvider)
    {
        _aElfClientProvider = aElfClientProvider;
    }
    
    public async Task<long> GetBalanceAsync(string chainId, string symbol, Address owner)
    {
        var client = _aElfClientProvider.GetClient(chainId);
        var paramGetBalance = new GetBalanceInput()
        {
            Symbol = symbol,
            Owner = owner
        };
        var address = (await client.GetContractAddressByNameAsync(
            HashHelper.ComputeFrom("AElf.ContractNames.Token"))).ToBase58();
        var transactionGetToken =
            await client.GenerateTransactionAsync(client.GetAddressFromPrivateKey(PrivateKey), address,
                "GetBalance",
                paramGetBalance);
        var txWithSignGetToken = client.SignTransaction(PrivateKey, transactionGetToken);
        var transactionGetTokenResult = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
        {
            RawTransaction = txWithSignGetToken.ToByteArray().ToHex()
        });
        return GetBalanceOutput.Parser.ParseFrom(
            ByteArrayHelper.HexStringToByteArray(transactionGetTokenResult)).Balance;
    }
}