using System.Collections.Generic;
using System.Linq;

namespace UTM_Market.Core.Entities;

/// <summary>
/// Representa el agregado raíz de una Venta.
/// Encapsula los detalles y el estado general de la transacción.
/// </summary>
/// <param name="SaleID">Identificador único de la venta (PK).</param>
/// <param name="Folio">Folio o número de recibo para la venta.</param>
public class Sale(int SaleID, string Folio)
{
    private readonly List<SaleDetail> _details = new();

    public int SaleID { get; } = SaleID;
    public string Folio { get; set; } = Folio;
    
    /// <summary>
    /// Fecha y hora en que se registró la venta. Se inicializa automáticamente.
    /// </summary>
    public DateTime SaleDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Estado actual de la venta (Pendiente, Completada, Cancelada).
    /// </summary>
    public SaleStatus Status { get; set; } = SaleStatus.Pending;

    /// <summary>
    /// Colección de solo lectura de los detalles de la venta.
    /// </summary>
    public IReadOnlyCollection<SaleDetail> Details => _details.AsReadOnly();

    /// <summary>
    /// Número total de artículos en la venta.
    /// </summary>
    public int TotalItems => _details.Sum(d => d.Quantity);

    /// <summary>
    /// Monto total de la venta, sumando todos los detalles.
    /// </summary>
    public decimal TotalSale => _details.Sum(d => d.TotalDetail);

    /// <summary>
    /// Agrega un nuevo producto a la venta.
    /// </summary>
    /// <param name="product">El producto a agregar.</param>
    /// <param name="quantity">La cantidad de producto.</param>
    public void AddDetail(Product product, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor que cero.", nameof(quantity));
        }

        var existingDetail = _details.FirstOrDefault(d => d.Product.ProductID == product.ProductID);

        if (existingDetail != null)
        {
            existingDetail.Quantity += quantity;
        }
        else
        {
            var newDetail = new SaleDetail(product, quantity);
            _details.Add(newDetail);
        }
    }
}
