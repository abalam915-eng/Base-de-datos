# Informe de Diseño de Entidades de Dominio: UTM_Market

Fecha: 23 de Febrero de 2026

**Autor:** Gemini, Arquitecto de Software Senior

Este documento describe el diseño y la implementación de la capa de entidades de dominio para el proyecto `UTM_Market`, siguiendo un enfoque de `Domain-Driven Design` (DDD) y `Clean Architecture`. Las entidades se han creado como POCOs (Plain Old CLR Objects) puros, sin dependencias de frameworks de persistencia, para garantizar un núcleo de negocio aislado, testeable y compatible con `Native AOT`.

## 1. Definición del `Enum` de Estatus de Venta

Se define una enumeración para representar los estados discretos del ciclo de vida de una venta, mejorando la legibilidad y evitando el uso de "magic strings".

**`src/Core/Entities/SaleStatus.cs`**
```csharp
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
```

## 2. Código Fuente de las Entidades de Dominio

A continuación se presenta el código completo para cada una de las entidades de dominio generadas.

### Entidad `Product`
Representa un producto en el inventario. Las reglas de negocio (invariantes) como la no negatividad del precio y el stock se protegen directamente en los `setters` de las propiedades.

**`src/Core/Entities/Product.cs`**
```csharp
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
```

### Entidad `SaleDetail`
Representa una línea en una venta. Es clave que esta entidad capture el `UnitPrice` en el momento de su creación para mantener la integridad histórica de la venta, independientemente de futuros cambios de precio en el `Product`.

**`src/Core/Entities/SaleDetail.cs`**
```csharp
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
```

### Entidad `Sale` (Agregado Raíz)
Actúa como el Agregado Raíz que gestiona la consistencia de la transacción completa. Contiene una lista de `SaleDetail` y expone métodos para manipular el estado del agregado, como `AddDetail`, mientras que las propiedades calculadas (`TotalItems`, `TotalSale`) proveen una vista agregada de los datos.

**`src/Core/Entities/Sale.cs`**
```csharp
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
    public DateTime SaleDate { get; private set; } = DateTime.Now;
    
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
```

## 3. Reducción de Boilerplate con la Palabra Clave `field`

La introducción de la palabra clave `field` en C# 14 representa un avance significativo para la implementación de lógica de negocio dentro de las entidades.

**Antes de C# 14**, para agregar validación a una propiedad auto-implementada, era necesario declarar manualmente un campo de respaldo privado. Esto generaba código "boilerplate" (repetitivo) que ensuciaba la clase:

```csharp
private decimal _price; // Campo de respaldo manual
public decimal Price
{
    get => _price;
    set
    {
        if (value < 0) throw new ArgumentException(...);
        _price = value;
    }
}
```

**Con C# 14**, la palabra clave `field` actúa como un alias para el campo de respaldo generado automáticamente por el compilador. Esto nos permite escribir lógica de validación directamente en el accesor `set` sin declarar el campo privado, resultando en un código más limpio, conciso y fácil de mantener, como se demostró en la entidad `Product`.
