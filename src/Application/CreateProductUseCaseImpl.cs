using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for creating a new product.
/// </summary>
public sealed class CreateProductUseCaseImpl(IProductRepository productRepository) : ICreateProductUseCase
{
    public async Task ExecuteAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        // Basic domain validation
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name is required.");

        if (string.IsNullOrWhiteSpace(product.SKU))
            throw new ArgumentException("Product SKU is required.");

        // Senior Validation: Check if SKU already exists
        var filter = new UTM_Market.Core.Filters.ProductFilter(Sku: product.SKU);
        await foreach (var existingProduct in productRepository.FindAsync(filter, cancellationToken))
        {
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"A product with SKU '{product.SKU}' already exists.");
            }
        }

        await productRepository.AddAsync(product, cancellationToken);
    }
}