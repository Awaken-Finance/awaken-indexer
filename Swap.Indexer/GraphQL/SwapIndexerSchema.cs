using AElfIndexer.Client.GraphQL;

namespace Swap.Indexer.GraphQL;

public class SwapIndexerSchema : AElfIndexerClientSchema<Query>
{
    public SwapIndexerSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}