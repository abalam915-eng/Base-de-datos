using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for retrieving a single product by its unique identifier.
/// </summary>
public interface IGetProductByIdUseCase
{
    /// <summary>
    /// Executes the use case to find a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The found domain product or null if not found.</returns>
    Task<Product?> ExecuteAsync(int id, CancellationToken cancellationToken = default);
}