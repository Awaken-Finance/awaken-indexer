using System.Text.RegularExpressions;
using Swap.Indexer.Entities;

namespace Swap.Indexer;

public class TokenHelper
{
    public static TokenType GetTokenType(string symbol)
    {
        int num = Regex.Matches(symbol, "-").Count;
        if (num == 1)
        {
            string[] arr = symbol.Split("-");
            long.TryParse(arr[1], out long itemId);
            if (itemId > 0)
            {
                return TokenType.NFTItem;
            }
            else
            {
                return TokenType.NFTCollection;
            }
        }
        else
        {
            return TokenType.Token;
        }
    }

    public static string GetNFTCollectionSymbol(string nftItemSymbol)
    {
        int num = Regex.Matches(nftItemSymbol, "-").Count;
        if (num == 1)
        {

            return nftItemSymbol.Substring(0, nftItemSymbol.LastIndexOf("-")) + "-0";
        }
        
        return "";
    }

    public static long GetNFTItemId(string nftItemSymbol)
    {
        int num = Regex.Matches(nftItemSymbol, "-").Count;
        if (num != 1)
        {
            return 0;
        }

        long.TryParse(nftItemSymbol.Substring(nftItemSymbol.LastIndexOf("-") + 1), out long tokenId);
        return tokenId;
    }
}