namespace UTM_Market.Infrastructure.Models.Data;

/// <summary>
/// Representa un registro de la tabla [DetalleVenta] en la base de datos.
/// Esta es una clase POCO parcial diseñada para mapeo manual y compatibilidad con Native AOT.
/// </summary>
public partial class DetalleVentaEntity
{
    public int DetalleID { get; set; }
    public int VentaID { get; set; }
    public int ProductoID { get; set; }

    /// <summary>
    /// Precio unitario del producto en el momento de la venta.
    /// El setter replica la restricción CHECK [PrecioUnitario] >= 0.
    /// </summary>
    public decimal PrecioUnitario
    {
        get => field;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(PrecioUnitario), "El precio unitario no puede ser negativo.");
            field = value;
        }
    }

    /// <summary>
    /// Cantidad de producto vendido.
    /// El setter replica la restricción CHECK [Cantidad] > 0.
    /// </summary>
    public int Cantidad
    {
        get => field;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(Cantidad), "La cantidad debe ser mayor a cero.");
            field = value;
        }
    }
    
    public decimal TotalDetalle { get; set; }
}
