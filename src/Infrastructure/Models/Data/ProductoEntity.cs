namespace UTM_Market.Infrastructure.Models.Data;

/// <summary>
/// Representa un registro de la tabla [Producto] en la base de datos.
/// Esta es una clase POCO parcial diseñada para mapeo manual y compatibilidad con Native AOT.
/// </summary>
public partial class ProductoEntity
{
    public int ProductoID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Marca { get; set; }

    /// <summary>
    /// Precio del producto, mapeado desde un DECIMAL(19,4).
    /// El setter replica la restricción CHECK [Precio] >= 0 de la base de datos.
    /// </summary>
    public decimal Precio
    {
        get => field;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(Precio), "El precio no puede ser negativo.");
            field = value;
        }
    }

    /// <summary>
    /// Stock del producto.
    /// El setter replica la restricción CHECK [Stock] >= 0 de la base de datos.
    /// </summary>
    public int Stock
    {
        get => field;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(Stock), "El stock no puede ser negativo.");
            field = value;
        }
    }
}
