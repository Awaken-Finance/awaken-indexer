using AeFinder.App.TestBase;
using Microsoft.Extensions.DependencyInjection;
using SwapIndexer.Processors;
using SwapIndexer.Providers;
using SwapIndexer.Tests.Provider;
using Volo.Abp.Modularity;

namespace SwapIndexer;

[DependsOn(
    typeof(AeFinderAppTestBaseModule),
    typeof(SwapIndexerModule))]
public class SwapIndexerTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AeFinderAppEntityOptions>(options => { options.AddTypes<SwapIndexerModule>(); });
        
        context.Services.AddSingleton<IAElfDataProvider, MockAElfDataProvider>();
        context.Services.AddSingleton<TradePairTokenOrderProvider>();
        context.Services.AddSingleton<PairCreatedProcessor>();
        context.Services.AddSingleton<PairCreatedProcessor2>();
        context.Services.AddSingleton<PairCreatedProcessor3>();
        context.Services.AddSingleton<PairCreatedProcessor4>();
        context.Services.AddSingleton<PairCreatedProcessor5>();
        context.Services.AddSingleton<LiquidityAddedProcessor>();
        context.Services.AddSingleton<LiquidityAddedProcessor2>();
        context.Services.AddSingleton<LiquidityAddedProcessor3>();
        context.Services.AddSingleton<LiquidityAddedProcessor4>();
        context.Services.AddSingleton<LiquidityAddedProcessor5>();
        context.Services.AddSingleton<LiquidityRemovedProcessor>();
        context.Services.AddSingleton<LiquidityRemovedProcessor2>();
        context.Services.AddSingleton<LiquidityRemovedProcessor3>();
        context.Services.AddSingleton<LiquidityRemovedProcessor4>();
        context.Services.AddSingleton<LiquidityRemovedProcessor5>();
        context.Services.AddSingleton<SwapProcessor>();
        context.Services.AddSingleton<SwapProcessor2>();
        context.Services.AddSingleton<SwapProcessor3>();
        context.Services.AddSingleton<SwapProcessor4>();
        context.Services.AddSingleton<SwapProcessor5>();
        context.Services.AddSingleton<SyncProcessor>();
        context.Services.AddSingleton<SyncProcessor2>();
        context.Services.AddSingleton<SyncProcessor3>();
        context.Services.AddSingleton<SyncProcessor4>();
        context.Services.AddSingleton<SyncProcessor5>();
        context.Services.AddSingleton<TokenTransferredLogEventProcessor>();
        context.Services.AddSingleton<TokenCrossChainReceivedProcessor>();
        context.Services.AddSingleton<TokenCrossChainTransferredProcessor>();
        context.Services.AddSingleton<TokenIssuedEventProcessor>();
        context.Services.AddSingleton<TokenBurnedEventProcessor>();
        context.Services.AddSingleton<HooksTransactionCreatedProcessor>();
        context.Services.AddSingleton<LimitOrderCreatedProcessor>();
        context.Services.AddSingleton<LimitOrderCancelledProcessor>();
        context.Services.AddSingleton<LimitOrderFilledProcessor>();
        context.Services.AddSingleton<LimitOrderRemovedProcessor>();
        context.Services.AddSingleton<LimitOrderTotalFilledProcessor>();
    }
}