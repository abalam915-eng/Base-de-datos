using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for updating an existing product's information.
/// </summary>
public interface IUpdateProductUseCase
{
    /// <summary>
    /// Executes the update logic for the specified product.
    /// </summary>
    /// <param name="product">The domain product with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task ExecuteAsync(Product product, CancellationToken cancellationToken = default);
}