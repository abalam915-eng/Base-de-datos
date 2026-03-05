using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;

namespace UTM_Market.Core.Repositories;

/// <summary>
/// Contrato que define las operaciones de persistencia para la entidad <see cref="Product"/>.
/// Está diseñado para ser completamente agnóstico a la tecnología de acceso a datos subyacente.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Obtiene todos los productos de forma asíncrona y en streaming.
    /// Ideal para procesar grandes volúmenes de datos con bajo consumo de memoria.
    /// </summary>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Product}"/> que permite iterar los productos a medida que se reciben de la fuente de datos.</returns>
    IAsyncEnumerable<Product> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un producto por su identificador único.
    /// </summary>
    /// <param name="id">El ID del producto a buscar.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Una tarea que representa la operación asíncrona. El resultado es el <see cref="Product"/> encontrado, o null si no existe.</returns>
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una búsqueda de productos basada en un conjunto de criterios.
    /// </summary>
    /// <param name="filter">El objeto con los criterios de filtro para la búsqueda.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Un <see cref="IAsyncEnumerable{Product}"/> con los productos que coinciden con los criterios de búsqueda.</returns>
    IAsyncEnumerable<Product> FindAsync(ProductFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega un nuevo producto a la fuente de datos.
    /// </summary>
    /// <param name="product">La entidad de dominio <see cref="Product"/> a agregar.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Una tarea que representa la operación asíncrona de guardado.</returns>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un producto existente en la fuente de datos.
    /// </summary>
    /// <param name="product">La entidad de dominio <see cref="Product"/> con los datos actualizados.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Una tarea que representa la operación asíncrona de actualización.</returns>
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el stock de un producto específico de forma atómica.
    /// Este método es preferible a `UpdateAsync` para evitar condiciones de carrera al modificar solo el stock.
    /// </summary>
    /// <param name="productId">El ID del producto a actualizar.</param>
    /// <param name="newStock">La nueva cantidad de stock.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task UpdateStockAsync(int productId, int newStock, CancellationToken cancellationToken = default);
}
