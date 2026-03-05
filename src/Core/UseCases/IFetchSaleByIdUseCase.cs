using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for fetching a specific sale by its unique identifier.
/// </summary>
public interface IFetchSaleByIdUseCase
{
    /// <summary>
    /// Executes the retrieval of a sale by ID.
    /// </summary>
    /// <param name="id">The sale identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A value task containing the sale if found; otherwise, null.</returns>
    ValueTask<Sale?> ExecuteAsync(int id, CancellationToken cancellationToken = default);
}