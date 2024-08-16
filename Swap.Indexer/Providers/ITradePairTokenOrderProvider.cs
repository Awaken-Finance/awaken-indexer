using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swap.Indexer.Options;
using Swap.Indexer.Processors;
using Volo.Abp.DependencyInjection;

namespace Swap.Indexer.Providers
{
    public interface ITradePairTokenOrderProvider
    {
        int GetTokenWeight(string tokenSymbol);
    }

    public class TradePairTokenOrderProvider : ITradePairTokenOrderProvider, ISingletonDependency
    {
        private readonly ILogger<TradePairTokenOrderProvider> _logger;
        private readonly TradePairTokenOrderOptions _tradePairTokenOrderOptions;
        private readonly Dictionary<string, TradePairToken> _tradePairTokenCache;

        public TradePairTokenOrderProvider(IOptionsSnapshot<TradePairTokenOrderOptions> tradePairTokenOrderOptions,
            ILogger<TradePairTokenOrderProvider> logger)
        {
            _logger = logger;
            _tradePairTokenOrderOptions = tradePairTokenOrderOptions.Value;
            _logger.LogInformation($"got trade pair token order config size: {_tradePairTokenOrderOptions.TradePairTokens}");
            
            _tradePairTokenCache = new Dictionary<string, TradePairToken>();
            int weight = _tradePairTokenOrderOptions.TradePairTokens.Count;
            foreach (var token in _tradePairTokenOrderOptions.TradePairTokens)
            {
                if (token.Weight == 0)
                {
                    token.Weight = weight--;
                }
                _tradePairTokenCache[token.Symbol] = token;
            }
            
            foreach (var token in _tradePairTokenCache)
            {
                _logger.LogInformation($"got trade pair token order cache, key:{token.Key}, address: {token.Value.Address}, symbol: {token.Value.Symbol}, weight: {token.Value.Weight}");
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
}