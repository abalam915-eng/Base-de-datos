namespace UTM_Market.Core.Entities;

/// <summary>
/// Define los posibles estados de una venta.
/// </summary>
public enum SaleStatus
{
    /// <summary>
    /// Venta iniciada pero no completada.
    /// </summary>
    Pending,
    
    /// <summary>
    /// Venta completada y pagada.
    /// </summary>
    Completed,
    
    /// <summary>
    /// Venta cancelada.
    /// </summary>
    Cancelled
}
