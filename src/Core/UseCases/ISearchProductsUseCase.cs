using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for searching products using dynamic filters.
/// </summary>
public interface ISearchProductsUseCase
{
    /// <summary>
    /// Executes the search based on the provided filter criteria.
    /// </summary>
    /// <param name="filter">Search criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of matching products.</returns>
    IAsyncEnumerable<Product> ExecuteAsync(ProductFilter filter, CancellationToken cancellationToken = default);
}