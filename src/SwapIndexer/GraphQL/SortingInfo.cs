using System.Linq.Expressions;

namespace SwapIndexer.GraphQL;

public class SortingInfo<T>
{
    public bool IsAscending { get; set; }
    public Expression<Func<T, object>> SortExpression { get; set; }
}