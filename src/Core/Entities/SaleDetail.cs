namespace UTM_Market.Core.Entities;

/// <summary>
/// Representa una línea de detalle dentro de una venta.
/// </summary>
/// <param name="Product">El producto que se está vendiendo.</param>
/// <param name="Quantity">La cantidad de producto vendido.</param>
public class SaleDetail(Product Product, int Quantity)
{
    public Product Product { get; } = Product ?? throw new ArgumentNullException(nameof(Product));
    public int Quantity { get; set; } = Quantity;

    /// <summary>
    /// Precio unitario del producto al momento de la venta.
    /// Se captura desde el producto para preservar el historial de precios.
    /// </summary>
    public decimal UnitPrice { get; } = Product.Price;

    /// <summary>
    /// Calcula el total para esta línea de detalle.
    /// Utiliza un miembro con cuerpo de expresión de C# para mayor concisión.
    /// </summary>
    public decimal TotalDetail => UnitPrice * Quantity;
}
