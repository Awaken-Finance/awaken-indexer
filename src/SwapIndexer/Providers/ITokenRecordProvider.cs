// using JetBrains.Annotations;
// using Nest;
// using SwapIndexer.Application.Contracts.Token;
// using SwapIndexer.Entities.Token;
// using Volo.Abp.ObjectMapping;
//
// namespace SwapIndexer.Providers;
//
// public interface ITokenRecordProvider
// {
//     public Task <List<TokenRecordIndexDto>> GetToken([CanBeNull] string tokenSymbol,[CanBeNull] string tokenId);
//
//     public Task<List<TokenRecordIndexDto>> GetTokenBySymbols(List<string> tokenSymbols);
// }
//
// public class TokenRecordProvider : ITokenRecordProvider
// {
//     protected readonly IObjectMapper ObjectMapper;
//     protected readonly IReadOnlyRepository<TokenRecordIndex, LogEventInfo> _tokenRepository;
//     
//     public TokenRecordProvider(
//         IObjectMapper objectMapper,
//         IReadOnlyRepository<TokenRecordIndex, LogEventInfo> tokenRepository)
//     {
//         ObjectMapper = objectMapper;
//         _tokenRepository = tokenRepository;
//     }
//
//     public async Task<List<TokenRecordIndexDto>> GetToken([CanBeNull] string tokenSymbol,[CanBeNull] string tokenId)
//     {
//         var mustQuery = new List<Func<QueryContainerDescriptor<TokenRecordIndex>, QueryContainer>>();
//         if (tokenSymbol != null)
//         {
//             mustQuery.Add(q => q.Term(i
//                 => i.Field(f => f.Symbol).Value(tokenSymbol)));
//         }
//         if (tokenId != null)
//         {
//             mustQuery.Add(q => q.Term(i
//                 => i.Field(f => f.Id).Value(tokenId)));
//         }
//
//         QueryContainer Filter(QueryContainerDescriptor<TokenRecordIndex> f) =>
//             f.Bool(b => b.Must(mustQuery));
//
//         var result = await _tokenRepository.GetListAsync(Filter, skip: 0, limit: 1000);
//         
//         return ObjectMapper.Map<List<TokenRecordIndex>, List<TokenRecordIndexDto>>(result.Item2);
//     }
//     
//     public async Task<List<TokenRecordIndexDto>> GetTokenBySymbols(List<string> tokenSymbols)
//     {
//         var mustQuery = new List<Func<QueryContainerDescriptor<TokenRecordIndex>, QueryContainer>>();
//         if (tokenSymbols != null && tokenSymbols.Any())
//         {
//             mustQuery.Add(q => q.Terms(i
//                 => i.Field(f => f.Symbol).Terms(tokenSymbols)));
//         }
//         QueryContainer Filter(QueryContainerDescriptor<TokenRecordIndex> f) =>
//             f.Bool(b => b.Must(mustQuery));
//         var result = await _tokenRepository.GetListAsync(Filter, skip: 0, limit: 1000);
//         return ObjectMapper.Map<List<TokenRecordIndex>, List<TokenRecordIndexDto>>(result.Item2);
//     }
// }