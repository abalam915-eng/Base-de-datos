namespace UTM_Market.Core.UseCases;

/// <summary>
/// Specialized Use Case for updating only the stock quantity of a product.
/// </summary>
public interface IUpdateProductStockUseCase
{
    /// <summary>
    /// Executes the atomic stock update.
    /// </summary>
    /// <param name="productId">Identifier of the product.</param>
    /// <param name="newStock">The new stock quantity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task ExecuteAsync(int productId, int newStock, CancellationToken cancellationToken = default);
}