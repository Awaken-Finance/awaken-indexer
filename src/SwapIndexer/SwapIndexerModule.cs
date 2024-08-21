using AeFinder.Sdk.Processor;
using SwapIndexer.GraphQL;
using SwapIndexer.Processors;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using SwapIndexer.Providers;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace SwapIndexer;

public class SwapIndexerModule: AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<SwapIndexerModule>(); });
        context.Services.AddSingleton<ISchema, SwapIndexerSchema>();
        
        context.Services.AddSingleton<IAElfDataProvider, AElfDataProvider>();
        context.Services.AddSingleton<ITradePairTokenOrderProvider, TradePairTokenOrderProvider>();
        
        context.Services.AddTransient<ILogEventProcessor, PairCreatedProcessor>();
        context.Services.AddTransient<ILogEventProcessor, PairCreatedProcessor2>();
        context.Services.AddTransient<ILogEventProcessor, PairCreatedProcessor3>();
        context.Services.AddTransient<ILogEventProcessor, PairCreatedProcessor4>();
        context.Services.AddTransient<ILogEventProcessor, PairCreatedProcessor5>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityAddedProcessor>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityAddedProcessor2>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityAddedProcessor3>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityAddedProcessor4>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityAddedProcessor5>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityRemovedProcessor>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityRemovedProcessor2>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityRemovedProcessor3>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityRemovedProcessor4>();
        context.Services.AddTransient<ILogEventProcessor, LiquidityRemovedProcessor5>();
        context.Services.AddTransient<ILogEventProcessor, SwapProcessor>();
        context.Services.AddTransient<ILogEventProcessor, SwapProcessor2>();
        context.Services.AddTransient<ILogEventProcessor, SwapProcessor3>();
        context.Services.AddTransient<ILogEventProcessor, SwapProcessor4>();
        context.Services.AddTransient<ILogEventProcessor, SwapProcessor5>();
        context.Services.AddTransient<ILogEventProcessor, SyncProcessor>();
        context.Services.AddTransient<ILogEventProcessor, SyncProcessor2>();
        context.Services.AddTransient<ILogEventProcessor, SyncProcessor3>();
        context.Services.AddTransient<ILogEventProcessor, SyncProcessor4>();
        context.Services.AddTransient<ILogEventProcessor, SyncProcessor5>();
        context.Services.AddTransient<ILogEventProcessor, TokenTransferredLogEventProcessor>();
        context.Services.AddTransient<ILogEventProcessor, TokenCrossChainReceivedProcessor>();
        context.Services.AddTransient<ILogEventProcessor, TokenCrossChainTransferredProcessor>();
        context.Services.AddTransient<ILogEventProcessor, TokenIssuedEventProcessor>();
        context.Services.AddTransient<ILogEventProcessor, TokenBurnedEventProcessor>();
        context.Services.AddTransient<ILogEventProcessor, HooksTransactionCreatedProcessor>();
        context.Services.AddTransient<ILogEventProcessor, LimitOrderCreatedProcessor>();
        context.Services.AddTransient<ILogEventProcessor, LimitOrderCancelledProcessor>();
        context.Services.AddTransient<ILogEventProcessor, LimitOrderFilledProcessor>();
        context.Services.AddTransient<ILogEventProcessor, LimitOrderRemovedProcessor>();
        context.Services.AddTransient<ILogEventProcessor, LimitOrderTotalFilledProcessor>();
    }
}