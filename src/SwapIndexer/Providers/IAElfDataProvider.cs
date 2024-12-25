using AeFinder.Sdk;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Google.Protobuf;

namespace SwapIndexer.Providers;

public interface IAElfDataProvider
{
    Task<long> GetBalanceAsync(string contractAddress, string chainId, string symbol, Address owner);
    
    Task<string> GetTokenUriAsync(string contractAddress, string chainId, string symbol);
}

public class AElfDataProvider : IAElfDataProvider
{
    private const string FTImageUriKey = "__ft_image_uri";
    private const string NFTImageUriKey = "__nft_image_uri";
    private const string NFTImageUrlKey = "__nft_image_url";
    private readonly IBlockChainService _blockChainService;
    
    public AElfDataProvider(IBlockChainService blockChainService)
    {
        _blockChainService = blockChainService;
    }
    
    public async Task<long> GetBalanceAsync(string contractAddress, string chainId, string symbol, Address owner)
    {
        var paramGetBalance = new GetBalanceInput()
        {
            Symbol = symbol,
            Owner = owner
        };
        var result =
            await _blockChainService.ViewContractAsync<GetBalanceOutput>(chainId, contractAddress, "GetBalance",
                paramGetBalance);
        return result.Balance;
    }

    public async Task<string> GetTokenUriAsync(string contractAddress, string chainId, string symbol)
    {
        var param = new GetTokenInfoInput()
        {
            Symbol = symbol
        };
        var result =
            await _blockChainService.ViewContractAsync<TokenInfo>(chainId, contractAddress, "GetTokenInfo",
                param);
        
        var externalInfo = result.ExternalInfo;
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