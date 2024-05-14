using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Swap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nethereum.Hex.HexConvertors.Extensions;
using Swap.Indexer.Application.Contracts.Token;
using Swap.Indexer.Entities;
using Swap.Indexer.GraphQL;
using Swap.Indexer.Options;
using Swap.Indexer.Processors;
using Swap.Indexer.Providers;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Swap.Indexer.Tests.Processors;

public class PairCreatedProcessorSub : PairCreatedProcessor
{
    public PairCreatedProcessorSub(ILogger<PairCreatedProcessor> logger, 
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions, 
        IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> repository,
        IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo> swapRecordIndexRepository,
        ITradePairTokenOrderProvider tradePairTokenOrderProvider)
        : base(logger, objectMapper, contractInfoOptions, repository, swapRecordIndexRepository, tradePairTokenOrderProvider)
    {
    }

    public async Task NewHandleEventAsync(PairCreated eventValue, LogEventContext context)
    {
        await base.HandleEventAsync(eventValue,context);
    }
}

public class PairCreatedProcessorTests : SwapIndexerTests
{
    private readonly IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo> _recordRepository;
    private readonly IObjectMapper _objectMapper;

    public PairCreatedProcessorTests()
    {
        _recordRepository = GetRequiredService<IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo>>();
        _objectMapper = GetRequiredService<IObjectMapper>();
    }

    [Fact]
    public async void PairCreatedAsycTest()
    {
        //mock processer
        var chainId = "AELF";
        var contractInfoOptions = new ContractInfoOptions();
        contractInfoOptions.ContractInfos = new List<ContractInfo>();
        contractInfoOptions.ContractInfos.Add(new ContractInfo
        {
            ChainId = chainId,
            SwapContractAddress = "0x123123123",
            FeeRate = 0.003,
            Level = 1
        });
        
        var loggerMock = new Mock<ILogger<PairCreatedProcessor>>();
        var objectMapperMock = new Mock<IObjectMapper>();
        var contractInfoOptionsMock = new Mock<IOptionsSnapshot<ContractInfoOptions>>();
        contractInfoOptionsMock.Setup(options => options.Value)
            .Returns(contractInfoOptions);
        var repositoryMock = new Mock<IAElfIndexerClientEntityRepository<TradePairInfoIndex, LogEventInfo>>();
        repositoryMock.Setup(repository => repository.AddOrUpdateAsync(It.IsAny<TradePairInfoIndex>()))
            .Callback<TradePairInfoIndex>(e =>
            {
                Assert.Equal("ELF", e.Token0Symbol);
                Assert.Equal("USDT", e.Token1Symbol);
                Assert.Equal(chainId, e.ChainId);
                Assert.True(e.IsTokenReversed);
            })
            .Returns(Task.CompletedTask);
        
        var swapRecordIndexMock = new Mock<IAElfIndexerClientEntityRepository<SwapRecordIndex, LogEventInfo>>();
        var tradePairTokenOrderProviderMock = new Mock<ITradePairTokenOrderProvider>();
        tradePairTokenOrderProviderMock.Setup(provider => provider.GetTokenWeight("USDT"))
            .Returns(100);
        tradePairTokenOrderProviderMock.Setup(provider => provider.GetTokenWeight("ELF"))
            .Returns(90);
        var tokenRecordProviderMock = new Mock<ITokenRecordProvider>();
        tokenRecordProviderMock.Setup(provider => provider.GetTokenBySymbols(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<TokenRecordIndexDto>
            {
                new TokenRecordIndexDto
                {
                    Id = "xxx-aaa",
                    Address = "oxs123123213",
                    Symbol = "USDT",
                },
                new TokenRecordIndexDto
                {
                    Id = "aaa-cccc",
                    Address = "oxs123123213",
                    Symbol = "ELF",
                },
            });

        var pairCreatedProcessorSub = new PairCreatedProcessorSub(
            loggerMock.Object,
            objectMapperMock.Object,
            contractInfoOptionsMock.Object,
            repositoryMock.Object,
            swapRecordIndexMock.Object,
            tradePairTokenOrderProviderMock.Object);

        var ContractAddress = "902F1767e142B27e9a068d00aAA8eA0D820B3591a1a2a3a4a5a6a7a8a9a1a2a1";
        var len = ContractAddress.HexToByteArray().Length;
        pairCreatedProcessorSub.NewHandleEventAsync(new PairCreated
        {
            SymbolA = "USDT",
            SymbolB = "ELF",
            Pair = Address.FromBytes(ContractAddress.HexToByteArray()),
        }, new LogEventContext
        {
            ChainId = chainId,
            TransactionId = "cccc-dddd",
        }).Wait();
        
        //检查es
        repositoryMock.Verify(repository => repository.AddOrUpdateAsync(It.IsAny<TradePairInfoIndex>()), Times.Once);
        await Query.TradePairInfoAsync(_recordRepository, _objectMapper, new GetTradePairInfoDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            ChainId = "ELF",
            Token0Symbol = "ELF",
            Token1Symbol = "USDT",
            TokenSymbol = "ELF",
            FeeRate = 1.0,
            Address = "AA",
            Id = "1"
        });
        
        await Query.GetTradePairInfoListAsync(_recordRepository, _objectMapper, new GetTradePairInfoDto()
        {
            SkipCount = 0,
            MaxResultCount = 100,
            ChainId = "ELF",
            Token0Symbol = "ELF",
            Token1Symbol = "USDT",
            TokenSymbol = "ELF",
            FeeRate = 1.0,
            Address = "AA",
            Id = "1"
        });
    }
}