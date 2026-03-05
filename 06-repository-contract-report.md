# Informe de Diseño: Contrato de Repositorio para `Product`

A continuación se presenta el diseño del contrato `IProductRepository` para la entidad de dominio `Product`, siguiendo las directrices de Clean Architecture y los requisitos para compatibilidad con .NET 10 Native AOT.

## 1. Estructura de Namespaces Recomendada

Se ha organizado el código para separar claramente las responsabilidades dentro de la capa `Core`:

- **`UTM_Market.Core.Entities`**: Contiene las clases de entidad de dominio puras (ej. `Product`).
- **`UTM_Market.Core.Repositories`**: Alberga las interfaces (contratos) de los repositorios (ej. `IProductRepository`).
- **`UTM_Market.Core.Filters`**: Contiene objetos inmutables utilizados para definir criterios de búsqueda (ej. `ProductFilter`).

Esta estructura asegura que la capa `Core` sea independiente de la infraestructura y define un límite claro para la lógica de negocio.

## 2. Código Fuente de `IProductRepository.cs`

Este contrato define las operaciones de negocio para la gestión de productos, utilizando tipos de dominio y patrones asíncronos modernos.

```csharp
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
```

## 3. Definición de `ProductFilter`

Para el método `FindAsync`, se creó un `record` inmutable que encapsula los criterios de búsqueda. Este enfoque es preferible a `Expression<Func<T, bool>>` porque no depende de la reflexión, garantizando la compatibilidad con Native AOT.

```csharp
namespace UTM_Market.Core.Filters;

/// <summary>
/// Define los criterios de búsqueda para productos.
/// Es un 'record' para garantizar inmutabilidad y simplicidad, ideal para Native AOT.
/// </summary>
/// <param name="Name">Filtra por nombre de producto (búsqueda parcial).</param>
/// <param name="Sku">Filtra por SKU exacto.</param>
/// <param name="MinPrice">Filtra por precio mínimo.</param>
/// <param name="MaxPrice">Filtra por precio máximo.</param>
public record ProductFilter(
    string? Name = null,
    string? Sku = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null);
```

## 4. Ventajas de `IAsyncEnumerable<T>` en Aplicaciones de Consola (.NET 10)

En una aplicación de consola como `UTM_Market`, que podría usarse para procesar lotes de datos o generar informes, el uso de `IAsyncEnumerable<T>` en métodos como `GetAllAsync` es superior a `Task<IEnumerable<T>>` por las siguientes razones:

1.  **Bajo Consumo de Memoria (Streaming)**: `IAsyncEnumerable<T>` no carga todos los resultados en memoria a la vez. En su lugar, los "transmite" (`yield return`) uno por uno desde la base de datos al consumidor. Si la tabla de productos tiene 1 millón de registros, una implementación con `Task<List<Product>>` podría consumir gigabytes de RAM, mientras que `IAsyncEnumerable<T>` mantendría un uso de memoria bajo y constante.

2.  **Menor Latencia hasta el Primer Resultado**: El procesamiento de datos puede comenzar tan pronto como el primer elemento está disponible. No es necesario esperar a que la consulta completa termine y todos los datos sean transferidos y deserializados en una colección. Esto mejora la capacidad de respuesta percibida de la aplicación.

3.  **Composición Asíncrona Natural**: El patrón `await foreach` permite un procesamiento asíncrono y secuencial del flujo de datos de una manera legible y mantenible, integrándose perfectamente con otras operaciones asíncronas dentro del bucle.

4.  **Cancelación Eficiente**: El `CancellationToken` se puede utilizar para detener la enumeración a mitad de camino, lo que no solo detiene el código del cliente, sino que también puede señalar al proveedor de datos (a través de Dapper) que cancele la consulta subyacente, ahorrando recursos de base de datos.

En resumen, `IAsyncEnumerable<T>` es la opción de diseño óptima para consultas que pueden devolver conjuntos de resultados grandes, ya que promueve la eficiencia de recursos y la escalabilidad, aspectos cruciales en aplicaciones de alto rendimiento.
