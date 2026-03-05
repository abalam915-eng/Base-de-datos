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
