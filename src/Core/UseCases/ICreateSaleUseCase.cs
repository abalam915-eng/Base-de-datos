using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for orchestrating the creation and persistence of a new sale.
/// </summary>
public interface ICreateSaleUseCase
{
    /// <summary>
    /// Executes the sale creation logic.
    /// </summary>
    /// <param name="sale">The domain sale object to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A value task containing the created sale with persisted metadata.</returns>
    ValueTask<Sale> ExecuteAsync(Sale sale, CancellationToken cancellationToken = default);
}