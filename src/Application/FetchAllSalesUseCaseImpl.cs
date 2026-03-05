using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for fetching all sales.
/// </summary>
public sealed class FetchAllSalesUseCaseImpl(ISaleRepository saleRepository) : IFetchAllSalesUseCase
{
    public IAsyncEnumerable<Sale> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return saleRepository.GetAllAsync(cancellationToken);
    }
}