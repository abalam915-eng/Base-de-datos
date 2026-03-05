# Informe de Diseño: Contrato de Repositorio para `Sale` (Venta)

Este documento detalla el diseño de la interfaz `ISaleRepository`, el contrato de persistencia para el agregado de dominio `Sale`. El diseño está optimizado para .NET 10, C# 14 y compilación Native AOT.

## 1. Código Fuente de `ISaleRepository.cs`

La interfaz define un contrato estricto en inglés, agnóstico a la implementación de persistencia, y se alinea con los patrones de diseño modernos de C#.

```csharp
using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;

namespace UTM_Market.Core.Repositories;

/// <summary>
/// Defines the repository contract for the Sale aggregate root.
/// This interface is the boundary for all persistence operations related to sales.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Retrieves all sales from the data store using an asynchronous stream.
    /// This method is memory-efficient for processing large datasets.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IAsyncEnumerable{Sale}"/> that streams sales from the data store.</returns>
    IAsyncEnumerable<Sale> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single sale by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. 
    /// The task result contains the <see cref="Sale"/> if found; otherwise, null.</returns>
    Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds sales that match the specified filter criteria, returning them as an async stream.
    /// </summary>
    /// <param name="filter">The criteria to apply to the search.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IAsyncEnumerable{Sale}"/> that streams matching sales.</returns>
    IAsyncEnumerable<Sale> FindAsync(SaleFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new sale to the data store.
    /// </summary>
    /// <param name="sale">The <see cref="Sale"/> aggregate root to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation.
    /// The task result is the persisted <see cref="Sale"/>, which may include a database-generated ID.</returns>
    Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing sale in the data store.
    /// </summary>
    /// <param name="sale">The <see cref="Sale"/> aggregate root with updated data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous update operation.</returns>
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
}
```

## 2. Definición del Objeto `SaleFilter`

Se ha definido un `record` inmutable para los criterios de búsqueda, lo cual es ideal para la compilación AOT al evitar la reflexión necesaria para interpretar árboles de expresión.

```csharp
using UTM_Market.Core.Entities;

namespace UTM_Market.Core.Filters;

/// <summary>
/// Defines the criteria for searching sales.
/// Designed as an immutable record for simplicity and Native AOT compatibility.
/// </summary>
/// <param name="StartDate">Filter for sales that occurred on or after this date.</param>
/// <param name="EndDate">Filter for sales that occurred on or before this date.</param>
/// <param name="Status">Filter by the status of the sale.</param>
public record SaleFilter(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    SaleStatus? Status = null);
```

## 3. Asesoría Arquitectónica

### Impacto de Native AOT en la Implementación del Repositorio

La decisión de compilar a **Native AOT (Ahead-Of-Time)** tiene implicaciones directas en la implementación de este contrato con Dapper:

1.  **Mapeo Manual Requerido**: Dapper depende en gran medida de la reflexión para mapear automáticamente los resultados de las consultas a objetos. En un entorno AOT, esta reflexión es limitada. La implementación de `ISaleRepository` deberá utilizar mapeo manual. Por ejemplo, en lugar de `db.QueryAsync<Sale>(...)`, se usará `db.QueryAsync(...)` y se construirá el objeto `Sale` y sus `SaleDetail` manualmente a partir de un `DbDataReader`.
2.  **Uso de Interceptores (C# 12+)**: Para centralizar la lógica de mapeo y hacerla más reutilizable, se puede emplear un **interceptor** de C# 12. Este podría interceptar una llamada a un método de extensión personalizado (ej. `db.QueryAsSalesAsync(...)`) y reemplazarla en tiempo de compilación por el código de mapeo manual optimizado, manteniendo el código del repositorio limpio.
3.  **SQL Explícito**: Las consultas SQL deben estar definidas explícitamente como strings. No se pueden usar bibliotecas que generen SQL dinámicamente a partir de expresiones (`IQueryable`), ya que estas también dependen de la reflexión.

### Advertencia: Riesgo de Consultas N+1

Al implementar métodos como `GetByIdAsync` o `FindAsync`, existe un riesgo significativo de introducir el **problema de rendimiento N+1**:

-   **Escenario de riesgo**: Una implementación ingenua podría primero ejecutar una consulta para obtener la `Venta` (1 consulta) y luego, dentro de un bucle, ejecutar una consulta separada por cada `Venta` para obtener sus `DetalleVenta` (N consultas).
-   **Solución Recomendada**: La implementación debe obtener tanto la venta como sus detalles en una **única consulta a la base de datos**. Con Dapper, esto se logra usando `QueryMultipleAsync`. La consulta SQL puede devolver dos conjuntos de resultados (uno para `VentaEntity` y otro para `DetalleVentaEntity`), que luego se ensamblan en un único agregado `Sale` en la capa de aplicación.

Este enfoque es crucial para garantizar que el repositorio sea escalable y no degrade el rendimiento de la base de datos a medida que aumenta el volumen de datos.
