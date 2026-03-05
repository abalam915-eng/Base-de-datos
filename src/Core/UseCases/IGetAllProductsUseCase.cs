using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for retrieving all products in the system.
/// </summary>
public interface IGetAllProductsUseCase
{
    /// <summary>
    /// Executes the use case to retrieve a stream of all products.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of domain products.</returns>
    IAsyncEnumerable<Product> ExecuteAsync(CancellationToken cancellationToken = default);
}