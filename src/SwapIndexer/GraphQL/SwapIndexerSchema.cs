using AeFinder.Sdk;

namespace SwapIndexer.GraphQL;

public class SwapIndexerSchema : AppSchema<Query>
{
    public SwapIndexerSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}