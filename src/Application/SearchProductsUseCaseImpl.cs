using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for searching products with filters.
/// </summary>
public sealed class SearchProductsUseCaseImpl(IProductRepository productRepository) : ISearchProductsUseCase
{
    public IAsyncEnumerable<Product> ExecuteAsync(ProductFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return productRepository.FindAsync(filter, cancellationToken);
    }
}