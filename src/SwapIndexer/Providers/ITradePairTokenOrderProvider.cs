using AeFinder.Sdk.Logging;
using Volo.Abp.DependencyInjection;

namespace SwapIndexer.Providers
{
    public interface ITradePairTokenOrderProvider
    {
        int GetTokenWeight(string tokenSymbol);
    }

    public class TradePairTokenOrderProvider : ITradePairTokenOrderProvider, ISingletonDependency
    {
        private readonly IAeFinderLogger _logger;
        private readonly Dictionary<string, TokenWeight> _tradePairTokenCache;

        public TradePairTokenOrderProvider(
            IAeFinderLogger logger)
        {
            _logger = logger;
            _logger.LogInformation($"got trade pair token order config size: {AwakenSwapConst.SortedTokenWeights.Count}");
            
            _tradePairTokenCache = new Dictionary<string, TokenWeight>();
            int weight = AwakenSwapConst.SortedTokenWeights.Count;
            foreach (var token in AwakenSwapConst.SortedTokenWeights)
            {
                _tradePairTokenCache[token] = new TokenWeight()
                {
                    Symbol = token,
                    Weight = weight--
                };
            }
            
            foreach (var token in _tradePairTokenCache)
            {
                _logger.LogInformation($"got trade pair token order cache, key: {token.Key}, symbol: {token.Value.Symbol}, weight: {token.Value.Weight}");
            }
        }

        public int GetTokenWeight(string tokenSymbol)
        {
            if (_tradePairTokenCache.TryGetValue(tokenSymbol, out var token))
            {
                return token.Weight;
            }

            return 0;
        }
    }
    
    public class TokenWeight
    {
        public string Symbol { get; set; }
        public int Weight { get; set; }
    }
}