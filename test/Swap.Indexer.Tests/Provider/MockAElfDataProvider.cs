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

    public async Task<long> GetTransactionFeeAsync(string chainId, string txnId)
    {
        switch (txnId)
        {
            case "e1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2":
            {
                return 2000;
            }
            case "i1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2":
            {
                return 1500;
            }
            case "g1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2":
            {
                return 1000;
            }
            default:
            {
                return 0;
            }
        }
        
    }
}