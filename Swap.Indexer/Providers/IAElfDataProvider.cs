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

    Task<long> GetDecimaleAsync(string chainId, string symbol);

    Task<long> GetTransactionFeeAsync(string chainId, string txnId);

    Task<string> GetTokenUriAsync(string chainId, string symbol);

}

public class AElfDataProvider : IAElfDataProvider
{
    private const string PrivateKey = "09da44778f8db2e602fb484334f37df19e221c84c4582ce5b7770ccfbc3ddbef";
    private const string FTImageUriKey = "__ft_image_uri";
    private const string NFTImageUriKey = "__nft_image_uri";
    private const string NFTImageUrlKey = "__nft_image_url";
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
    
    
    public async Task<long> GetDecimaleAsync(string chainId, string symbol)
    {
        var client = _aElfClientProvider.GetClient(chainId);
        var paramGetBalance = new GetBalanceInput()
        {
            Symbol = symbol
        };
        var address = (await client.GetContractAddressByNameAsync(
            HashHelper.ComputeFrom("AElf.ContractNames.Token"))).ToBase58();
        var transactionGetToken =
            await client.GenerateTransactionAsync(client.GetAddressFromPrivateKey(PrivateKey), address,
                "GetTokenInfo",
                paramGetBalance);
        var txWithSignGetToken = client.SignTransaction(PrivateKey, transactionGetToken);
        var transactionGetTokenResult = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
        {
            RawTransaction = txWithSignGetToken.ToByteArray().ToHex()
        });
        
        return TokenInfo.Parser.ParseFrom(
            ByteArrayHelper.HexStringToByteArray(transactionGetTokenResult)).Decimals;
    }


    public async Task<long> GetTransactionFeeAsync(string chainId, string transactionId)
    {
        try
        {
            var client = _aElfClientProvider.GetClient(chainId);
            var result = await client.GetTransactionResultAsync(transactionId);
            if (result == null)
            {
                return 0;
            }

            var transactionFeeCharged = TransactionFeeCharged.Parser.ParseFrom(
                ByteString.FromBase64(result.Logs.First(l => l.Name == nameof(TransactionFeeCharged)).NonIndexed));
            return transactionFeeCharged.Amount;
        }
        catch (Exception e)
        {
            return 0;
        }
    }

    public async Task<string> GetTokenUriAsync(string chainId, string symbol)
    {
        var client = _aElfClientProvider.GetClient(chainId);
        var paramGetBalance = new GetTokenInfoInput()
        {
            Symbol = symbol
        };
        var address = (await client.GetContractAddressByNameAsync(
            HashHelper.ComputeFrom("AElf.ContractNames.Token"))).ToBase58();
        var transactionGetToken =
            await client.GenerateTransactionAsync(client.GetAddressFromPrivateKey(PrivateKey), address,
                "GetTokenInfo",
                paramGetBalance);
        var txWithSignGetToken = client.SignTransaction(PrivateKey, transactionGetToken);
        var transactionGetTokenResult = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
        {
            RawTransaction = txWithSignGetToken.ToByteArray().ToHex()
        });
        
        var externalInfo = TokenInfo.Parser.ParseFrom(
            ByteArrayHelper.HexStringToByteArray(transactionGetTokenResult)).ExternalInfo;
        if (externalInfo != null && externalInfo.Value != null)
        {
            if (externalInfo.Value.ContainsKey(FTImageUriKey))
            {
                return externalInfo.Value[FTImageUriKey];
            }
            if (externalInfo.Value.ContainsKey(NFTImageUriKey))
            {
                return externalInfo.Value[NFTImageUriKey];
            }
            if (externalInfo.Value.ContainsKey(NFTImageUrlKey))
            {
                return externalInfo.Value[NFTImageUrlKey];
            }
        }
        return null;

    }
}