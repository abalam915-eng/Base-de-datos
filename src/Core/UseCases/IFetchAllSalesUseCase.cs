using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for fetching all sales records from the system.
/// </summary>
public interface IFetchAllSalesUseCase
{
    /// <summary>
    /// Executes the retrieval of all sales as an asynchronous stream.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of domain sales.</returns>
    IAsyncEnumerable<Sale> ExecuteAsync(CancellationToken cancellationToken = default);
}