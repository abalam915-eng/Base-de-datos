using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for retrieving a product by ID.
/// </summary>
public sealed class GetProductByIdUseCaseImpl(IProductRepository productRepository) : IGetProductByIdUseCase
{
    public async Task<Product?> ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) return null;
        return await productRepository.GetByIdAsync(id, cancellationToken);
    }
}