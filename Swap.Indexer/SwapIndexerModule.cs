using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Order;
using Microsoft.Extensions.DependencyInjection;
using Swap.Indexer.GraphQL;
using Swap.Indexer.Options;
using Swap.Indexer.Processors;
using Swap.Indexer.Providers;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Swap.Indexer;

[DependsOn(typeof(AElfIndexerClientModule))]
public class SwapIndexerModule : AElfIndexerClientPluginBaseModule<SwapIndexerModule, SwapIndexerSchema, Query>
{
    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SwapIndexerModule>();
        });
        var configuration = serviceCollection.GetConfiguration();
        serviceCollection.AddSingleton<IAElfDataProvider, AElfDataProvider>();
        serviceCollection.AddSingleton<ITradePairTokenOrderProvider, TradePairTokenOrderProvider>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, PairCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, PairCreatedProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, PairCreatedProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, PairCreatedProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, PairCreatedProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityAddedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityAddedProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityAddedProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityAddedProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityAddedProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityRemovedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityRemovedProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityRemovedProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityRemovedProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LiquidityRemovedProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SwapProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SwapProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SwapProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SwapProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SwapProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SyncProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SyncProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SyncProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SyncProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SyncProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenTransferredLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCrossChainReceivedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCrossChainTransferredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenIssuedEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenBurnedEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, HooksTransactionCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LimitOrderCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LimitOrderCancelledProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LimitOrderFilledProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, LimitOrderRemovedProcessor>();
        Configure<ContractInfoOptions>(configuration.GetSection("ContractInfo"));
        Configure<NodeOptions>(configuration.GetSection("Node"));
        Configure<TradePairTokenOrderOptions>(configuration.GetSection("TradePairTokenOrderOptions"));
        Configure<TotalValueLockedOptions>(configuration.GetSection("TotalValueLockedOptions"));
    }


    protected override string ClientId => "AElfIndexer_Swap";
    protected override string Version => "7084a134581b43c5a2d9bf2a02bafbd1";
}