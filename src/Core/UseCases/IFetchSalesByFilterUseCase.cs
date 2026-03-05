using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for fetching sales records matching specific criteria.
/// </summary>
public interface IFetchSalesByFilterUseCase
{
    /// <summary>
    /// Executes the retrieval of sales based on a filter.
    /// </summary>
    /// <param name="filter">The domain sale filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of matching domain sales.</returns>
    IAsyncEnumerable<Sale> ExecuteAsync(SaleFilter filter, CancellationToken cancellationToken = default);
}