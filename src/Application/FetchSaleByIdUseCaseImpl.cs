using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for fetching a sale by ID.
/// </summary>
public sealed class FetchSaleByIdUseCaseImpl(ISaleRepository saleRepository) : IFetchSaleByIdUseCase
{
    public async ValueTask<Sale?> ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0) return null;
        return await saleRepository.GetByIdAsync(id, cancellationToken);
    }
}