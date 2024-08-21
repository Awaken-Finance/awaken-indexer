// using AElf.Types;
// using AeFinder.App.TestBase;
// using AeFinder.Sdk;
// using AeFinder.Sdk.Processor;
// using Nethereum.Hex.HexConvertors.Extensions;
// using SwapIndexer.Entities;
// using SwapIndexer.Processors;
// using Volo.Abp.ObjectMapping;
// using Xunit;
//
// namespace SwapIndexer.Tests.Processors;
//
//
// public sealed class TokenProcessorTests : SwapIndexerTestBase
// {
//     private readonly IReadOnlyRepository<SwapUserTokenIndex> _repository;
//     private readonly IObjectMapper _objectMapper;
//
//     public TokenProcessorTests()
//     {
//         _repository = GetRequiredService<IReadOnlyRepository<SwapUserTokenIndex>>();
//         _objectMapper = GetRequiredService<IObjectMapper>();
//     }
//
//     [Fact]
//     public async Task TokenAsyncTests()
//     {
//         //step1: create blockStateSet
//         const string chainId = "AELF";
//         const string blockHash = "DefaultBlockHash";
//         const string previousBlockHash = "DefaultPreviousBlockHash";
//         const string transactionId = "DefaultTransactionId";
//         const long blockHeight = 100;
//         var blockStateSet = new BlockStateSet<LogEventInfo>
//         {
//             BlockHash = blockHash,
//             BlockHeight = blockHeight,
//             Confirmed = true,
//             PreviousBlockHash = previousBlockHash,
//         };
//         var blockStateSetTransaction = new BlockStateSet<TransactionInfo>
//         {
//             BlockHash = blockHash,
//             BlockHeight = blockHeight,
//             Confirmed = true,
//             PreviousBlockHash = previousBlockHash,
//         };
//         
//         var blockStateSetKey = await InitializeBlockStateSetAsync(blockStateSet, chainId);
//         var blockStateSetKeyTransaction = await InitializeBlockStateSetAsync(blockStateSetTransaction, chainId);
//         
//         // step2: create logEventInfo
//         var tokenBurnedEvent = new AElf.Contracts.MultiToken.Burned()
//         {
//             Burner = Address.FromPublicKey("AAA".HexToByteArray()),
//             Symbol = "AELF"
//         };
//         var tokenCrossChainReceivedEvent = new AElf.Contracts.MultiToken.CrossChainReceived()
//         {
//             From = Address.FromPublicKey("AAA".HexToByteArray()),
//             To = Address.FromPublicKey("BBB".HexToByteArray()),
//             Symbol = "AELF"
//         };
//         var tokenCrossChainTransferredEvent = new AElf.Contracts.MultiToken.CrossChainTransferred()
//         {
//             From = Address.FromPublicKey("AAA".HexToByteArray()),
//             To = Address.FromPublicKey("BBB".HexToByteArray()),
//             Symbol = "AELF-ETH"
//         };
//         var tokenIssuedEvent = new AElf.Contracts.MultiToken.Issued()
//         {
//             To = Address.FromPublicKey("BBB".HexToByteArray()),
//             Symbol = "AELF-2"
//         };
//         
//         var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenBurnedEvent.ToLogEvent());
//         logEventInfo.BlockHeight = blockHeight;
//         logEventInfo.ChainId= chainId;
//         logEventInfo.BlockHash = blockHash;
//         logEventInfo.TransactionId = transactionId;
//         
//         var logEventInfo2 = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCrossChainReceivedEvent.ToLogEvent());
//         logEventInfo2.BlockHeight = blockHeight;
//         logEventInfo2.ChainId= chainId;
//         logEventInfo2.BlockHash = blockHash;
//         logEventInfo2.TransactionId = transactionId;
//         
//         var logEventInfo3 = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenCrossChainTransferredEvent.ToLogEvent());
//         logEventInfo3.BlockHeight = blockHeight;
//         logEventInfo3.ChainId= chainId;
//         logEventInfo3.BlockHash = blockHash;
//         logEventInfo3.TransactionId = transactionId;
//         
//         var logEventInfo4 = LogEventHelper.ConvertAElfLogEventToLogEventInfo(tokenIssuedEvent.ToLogEvent());
//         logEventInfo4.BlockHeight = blockHeight;
//         logEventInfo4.ChainId= chainId;
//         logEventInfo4.BlockHash = blockHash;
//         logEventInfo4.TransactionId = transactionId;
//         var logEventContext = new LogEventContext
//         {
//             ChainId = chainId,
//             BlockHeight = blockHeight,
//             BlockHash = blockHash,
//             PreviousBlockHash = previousBlockHash,
//             TransactionId = transactionId,
//             Signature = "AELF",
//             Params = "{ \"to\": \"swap\", \"symbol\": \"ELF\", \"amount\": \"100000000000\" }",
//             MethodName = "Swap",
//             ExtraProperties = new Dictionary<string, string>
//             {
//                 { "TransactionFee", "{\"ELF\":\"3\"}" },
//                 { "ResourceFee", "{\"ELF\":\"3\"}" }
//             },
//             BlockTime = DateTime.UtcNow
//         };
//         
//         //step3: handle event and write result to blockStateSet
//         var tokenBurnedEventProcessor = GetRequiredService<TokenBurnedEventProcessor>();
//         await tokenBurnedEventProcessor.HandleEventAsync(logEventInfo, logEventContext);
//         
//         var tokenCrossChainReceivedProcessor = GetRequiredService<TokenCrossChainReceivedProcessor>();
//         await tokenCrossChainReceivedProcessor.HandleEventAsync(logEventInfo2, logEventContext);
//         
//         var tokenCrossChainTransferredProcessor = GetRequiredService<TokenCrossChainTransferredProcessor>();
//         await tokenCrossChainTransferredProcessor.HandleEventAsync(logEventInfo3, logEventContext);
//         
//         var tokenIssuedEventProcessor = GetRequiredService<TokenIssuedEventProcessor>();
//         await tokenIssuedEventProcessor.HandleEventAsync(logEventInfo4, logEventContext);
//     }
// }