using System.Collections.Generic;
using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementación concreta del caso de uso <see cref="ILowStockAlertUseCase"/>.
/// Utiliza C# 14 'Primary Constructors' para la inyección de dependencias.
/// </summary>
/// <param name="productRepository">El repositorio encargado de la persistencia de productos.</param>
public sealed class LowStockAlertUseCaseImpl(IProductRepository productRepository) : ILowStockAlertUseCase
{
    /// <summary>
    /// Ejecuta la lógica de filtrado de productos con bajo inventario.
    /// </summary>
    /// <param name="threshold">El umbral de stock crítico. Debe ser un valor positivo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Flujo de productos que cumplen con el criterio de stock crítico.</returns>
    /// <exception cref="ArgumentException">Se lanza si el umbral es menor o igual a cero.</exception>
    public async IAsyncEnumerable<Product> ExecuteAsync(int threshold, CancellationToken cancellationToken = default)
    {
        // Validación de entrada conforme a las restricciones técnicas
        if (threshold <= 0)
        {
            throw new ArgumentException("El umbral (threshold) debe ser mayor a cero.", nameof(threshold));
        }

        // Recuperamos todos los productos y filtramos en streaming para optimizar memoria
        await foreach (var product in productRepository.GetAllAsync(cancellationToken))
        {
            if (product.Stock <= threshold)
            {
                yield return product;
            }
        }
    }
}
