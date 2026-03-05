using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for creating a sale.
/// </summary>
public sealed class CreateSaleUseCaseImpl(
    ISaleRepository saleRepository,
    IProductRepository productRepository) : ICreateSaleUseCase
{
    public async ValueTask<Sale> ExecuteAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sale);
        
        if (string.IsNullOrWhiteSpace(sale.Folio))
            throw new ArgumentException("Sale folio is required.");

        if (!sale.Details.Any())
            throw new InvalidOperationException("Sale must have at least one detail.");

        // Senior Validation: Check product existence and stock for each detail
        foreach (var detail in sale.Details)
        {
            var product = await productRepository.GetByIdAsync(detail.Product.ProductID, cancellationToken);
            
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {detail.Product.ProductID} (SKU: {detail.Product.SKU}) not found.");
            }

            if (product.Stock < detail.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Requested: {detail.Quantity}, Available: {product.Stock}");
            }
            
            // Note: In a production environment, we should also update the product stock here or within a transaction
        }

        return await saleRepository.AddAsync(sale, cancellationToken);
    }
}