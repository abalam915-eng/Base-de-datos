using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for updating product stock.
/// </summary>
public sealed class UpdateProductStockUseCaseImpl(IProductRepository productRepository) : IUpdateProductStockUseCase
{
    public async Task ExecuteAsync(int productId, int newStock, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
            throw new ArgumentException("Invalid product ID.");

        if (newStock < 0)
            throw new ArgumentException("Stock cannot be negative.");

        // Senior Validation: Check product existence before update
        var existingProduct = await productRepository.GetByIdAsync(productId, cancellationToken);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product with ID {productId} not found.");
        }

        await productRepository.UpdateStockAsync(productId, newStock, cancellationToken);
    }
}