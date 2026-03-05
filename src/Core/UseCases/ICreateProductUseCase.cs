using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for creating a new product in the system.
/// </summary>
public interface ICreateProductUseCase
{
    /// <summary>
    /// Executes the creation logic for a new product.
    /// </summary>
    /// <param name="product">The domain product to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task ExecuteAsync(Product product, CancellationToken cancellationToken = default);
}