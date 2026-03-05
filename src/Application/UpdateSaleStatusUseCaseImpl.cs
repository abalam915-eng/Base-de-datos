using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for updating sale status.
/// </summary>
public sealed class UpdateSaleStatusUseCaseImpl(ISaleRepository saleRepository) : IUpdateSaleStatusUseCase
{
    public async ValueTask ExecuteAsync(int saleId, SaleStatus newStatus, CancellationToken cancellationToken = default)
    {
        if (saleId <= 0)
            throw new ArgumentException("Invalid sale ID.");

        var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {saleId} not found.");

        sale.Status = newStatus;
        await saleRepository.UpdateAsync(sale, cancellationToken);
    }
}