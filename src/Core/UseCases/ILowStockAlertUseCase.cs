using System.Collections.Generic;
using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Contrato para el caso de uso de alertas de inventario crítico.
/// Define la operación para obtener productos con bajo nivel de stock.
/// </summary>
public interface ILowStockAlertUseCase
{
    /// <summary>
    /// Obtiene los productos cuyo stock sea menor o igual al umbral especificado.
    /// </summary>
    /// <param name="threshold">El nivel de stock máximo para ser considerado una alerta.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Un flujo asíncrono (<see cref="IAsyncEnumerable{Product}"/>) con los productos filtrados.</returns>
    IAsyncEnumerable<Product> ExecuteAsync(int threshold, CancellationToken cancellationToken = default);
}
