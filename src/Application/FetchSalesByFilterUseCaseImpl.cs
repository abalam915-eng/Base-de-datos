using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for fetching sales by filter.
/// </summary>
public sealed class FetchSalesByFilterUseCaseImpl(ISaleRepository saleRepository) : IFetchSalesByFilterUseCase
{
    public IAsyncEnumerable<Sale> ExecuteAsync(SaleFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return saleRepository.FindAsync(filter, cancellationToken);
    }
}