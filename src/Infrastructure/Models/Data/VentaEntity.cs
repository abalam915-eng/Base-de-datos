namespace UTM_Market.Infrastructure.Models.Data;

/// <summary>
/// Representa un registro de la tabla [Venta] en la base de datos.
/// Esta es una clase POCO parcial diseñada para mapeo manual y compatibilidad con Native AOT.
/// </summary>
public partial class VentaEntity
{
    public int VentaID { get; set; }
    public string Folio { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }

    /// <summary>
    /// Total de artículos en la venta.
    /// El setter replica la restricción CHECK [TotalArticulos] >= 0.
    /// </summary>
    public int TotalArticulos
    {
        get => field;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(TotalArticulos), "El total de artículos no puede ser negativo.");
            field = value;
        }
    }

    /// <summary>
    /// Monto total de la venta.
    /// El setter replica la restricción CHECK [TotalVenta] >= 0.
    /// </summary>
    public decimal TotalVenta
    {
        get => field;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(TotalVenta), "El total de la venta no puede ser negativo.");
            field = value;
        }
    }

    /// <summary>
    /// Estatus de la venta, mapeado desde un TINYINT.
    /// 1: Pendiente, 2: Completada, 3: Cancelada.
    /// </summary>
    public byte Estatus { get; set; }
}
