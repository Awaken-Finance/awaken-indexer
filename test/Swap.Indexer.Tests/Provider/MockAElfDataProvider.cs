using AElf.Types;
using Swap.Indexer.Providers;

namespace Swap.Indexer.Tests.Provider;

public class MockAElfDataProvider : IAElfDataProvider
{
    public async Task<long> GetBalanceAsync(string chainId, string symbol, Address owner)
    {
        switch (chainId)
        {
            case "AELF":
                if ("USDT" == symbol)
                {
                    return 100;
                }

                if ("BTC" == symbol)
                {
                    return 200;
                }

                break;
            default:
                return 0;
        }

        return 0;
    }
    
    public async Task<long> GetDecimaleAsync(string chainId, string symbol)
    {
        if ("USDT" == symbol)
        {
            return 6;
        }

        if ("ELF" == symbol)
        {
            return 8;
        }
        return 0;
    }

    public async Task<string> GetTokenUriAsync(string chainId, string symbol)
    {
        return null;
    }
}