using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for retrieving all products.
/// </summary>
public sealed class GetAllProductsUseCaseImpl(IProductRepository productRepository) : IGetAllProductsUseCase
{
    public IAsyncEnumerable<Product> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return productRepository.GetAllAsync(cancellationToken);
    }
}