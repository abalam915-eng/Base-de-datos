using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for updating only the status property of an existing sale.
/// </summary>
public interface IUpdateSaleStatusUseCase
{
    /// <summary>
    /// Executes the status update for a sale.
    /// </summary>
    /// <param name="saleId">The identifier of the sale to update.</param>
    /// <param name="newStatus">The target status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A value task representing the operation.</returns>
    ValueTask ExecuteAsync(int saleId, SaleStatus newStatus, CancellationToken cancellationToken = default);
}

/// <summary>
/// Container for C# 14 Extension Blocks.
/// </summary>
public static class SalesUseCaseExtensions
{
    /// <summary>
    /// C# 14 Extension Block for semantic helpers at the interface level.
    /// </summary>
    extension (IUpdateSaleStatusUseCase useCase)
    {
        /// <summary>
        /// Convenience method to cancel a sale.
        /// </summary>
        ValueTask CancelSaleAsync(int saleId, CancellationToken ct = default) 
            => useCase.ExecuteAsync(saleId, SaleStatus.Cancelled, ct);

        /// <summary>
        /// Convenience method to complete a sale.
        /// </summary>
        ValueTask CompleteSaleAsync(int saleId, CancellationToken ct = default) 
            => useCase.ExecuteAsync(saleId, SaleStatus.Completed, ct);
    }
}