using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for updating a product.
/// </summary>
public sealed class UpdateProductUseCaseImpl(IProductRepository productRepository) : IUpdateProductUseCase
{
    public async Task ExecuteAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        if (product.ProductID <= 0)
            throw new ArgumentException("Invalid product ID.");

        // Senior Validation: Check product existence before update
        var existingProduct = await productRepository.GetByIdAsync(product.ProductID, cancellationToken);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product with ID {product.ProductID} not found.");
        }

        await productRepository.UpdateAsync(product, cancellationToken);
    }
}