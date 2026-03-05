namespace UTM_Market.Core.Entities;

/// <summary>
/// Representa un producto en el inventario.
/// Es una entidad pura de dominio, sin dependencias de infraestructura.
/// </summary>
/// <param name="ProductID">Identificador único del producto (PK).</param>
/// <param name="Name">Nombre del producto.</param>
/// <param name="SKU">Stock Keeping Unit.</param>
/// <param name="Brand">Marca del producto.</param>
public class Product(int ProductID, string Name, string SKU, string Brand)
{
    public int ProductID { get; } = ProductID;
    public string Name { get; set; } = Name;
    public string SKU { get; set; } = SKU;
    public string Brand { get; set; } = Brand;

    /// <summary>
    /// Precio de venta del producto.
    /// </summary>
    public decimal Price
    {
        get => field;
        set
        {
            // C# 14 'field' keyword: permite la validación directa en el 'setter'
            // sin necesidad de un campo de respaldo explícito (_price), reduciendo boilerplate.
            if (value < 0)
            {
                throw new ArgumentException("El precio no puede ser negativo.", nameof(Price));
            }
            field = value;
        }
    }

    /// <summary>
    /// Cantidad de existencias disponibles del producto.
    /// </summary>
    public int Stock
    {
        get => field;
        set
        {
            // La misma técnica con 'field' se usa para proteger la invariante de negocio
            // de que el stock no puede ser negativo.
            if (value < 0)
            {
                throw new ArgumentException("El stock no puede ser negativo.", nameof(Stock));
            }
            field = value;
        }
    }
}
