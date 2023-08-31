using Orleans.TestingHost;
using Swap.Indexer.TestBase;
using Volo.Abp.Modularity;

namespace Swap.Indexer.Orleans.TestBase;

public abstract class SwapIndexerOrleansTestBase<TStartupModule> : SwapIndexerTestBase<TStartupModule> 
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public SwapIndexerOrleansTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}