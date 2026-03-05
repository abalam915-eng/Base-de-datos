# Informe de Diseño de Entidades de Persistencia (AOT-Ready)

Fecha: 23 de Febrero de 2026

**Autor:** Gemini, Arquitecto de Software Senior

Este documento detalla la creación de las entidades de persistencia para el proyecto `UTM_Market`. Estas clases están diseñadas como un mapeo 1:1 del esquema de la base de datos y están optimizadas para el máximo rendimiento y compatibilidad con `Native AOT` en .NET 10, evitando el uso de reflexión.

## 1. Código Fuente de las Entidades de Persistencia

A continuación se presenta el código de las tres entidades generadas, que reflejan las tablas `Producto`, `Venta` y `DetalleVenta` de SQL Server.

### Entidad `ProductoEntity`
Mapea la tabla `Producto`. Las validaciones de `Precio` y `Stock` replican las restricciones `CHECK` de la base de datos.

**`src/Infrastructure/Models/Data/ProductoEntity.cs`**
```csharp
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
```

### Entidad `VentaEntity`
Mapea la tabla `Venta`. El campo `Estatus` se mapea a `byte` para corresponder con el tipo `TINYINT` de la base de datos.

**`src/Infrastructure/Models/Data/VentaEntity.cs`**
```csharp
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
```

### Entidad `DetalleVentaEntity`
Mapea la tabla `DetalleVenta`, la cual representa la tabla de unión en la relación muchos a muchos.

**`src/Infrastructure/Models/Data/DetalleVentaEntity.cs`**
```csharp
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
```

## 2. Nota Técnica: Mapeo Manual vs. ORMs Dinámicos en Entornos Native AOT

En el contexto de .NET 10 y la compilación `Native AOT` (Ahead-of-Time), la elección de la estrategia de acceso a datos es crítica para el rendimiento y la compatibilidad.

**ORMs Dinámicos (Ej. EF Core, Dapper estándar):**
Estos frameworks proporcionan una gran comodidad al mapear automáticamente los resultados de una consulta a objetos C# (POCOs). Sin embargo, esta "magia" se basa fundamentalmente en la **reflexión** en tiempo de ejecución. El ORM inspecciona el objeto, descubre sus propiedades por nombre y tipo, y las utiliza para leer los datos de la consulta.

El problema con este enfoque en un entorno AOT es doble:
1.  **Incompatibilidad con Trimming:** El compilador AOT realiza un "tree trimming" agresivo, eliminando cualquier código que no se use explícitamente. Como la reflexión puede invocar tipos y miembros de forma dinámica, el compilador no puede saber con certeza qué código es seguro eliminar. Esto a menudo conduce a excepciones en tiempo de ejecución cuando el ORM intenta acceder a un miembro que ha sido eliminado.
2.  **Sobrecarga de Rendimiento:** La reflexión es inherentemente más lenta que el acceso directo al código. Aunque los ORMs modernos la optimizan, sigue representando una sobrecarga que no existe en el código compilado estáticamente.

**Mapeo Manual (Nuestra Elección):**
La estrategia adoptada en este proyecto es realizar el mapeo manualmente desde un `SqlDataReader`. El código de mapeo se verá así:

```csharp
// Ejemplo conceptual de mapeo manual
var producto = new ProductoEntity
{
    ProductoID = reader.GetInt32(0),
    Nombre = reader.GetString(1),
    // ...etc
};
```

**Ventajas del Mapeo Manual en AOT:**
- **Rendimiento Máximo:** Es el método de acceso a datos más rápido posible, ya que se traduce en llamadas directas a métodos sin ninguna sobrecarga de reflexión o indirección.
- **Totalmente Compatible con AOT:** El código es explícito y estático. El compilador AOT puede ver exactamente qué métodos y tipos se utilizan, lo que le permite realizar un "trimming" seguro y eficaz sin riesgo de romper la aplicación.
- **Control Total:** Ofrece un control granular sobre cómo se leen y materializan los datos, lo cual es crucial para optimizaciones de bajo nivel.

Aunque requiere un poco más de código inicial, el mapeo manual garantiza la creación de las aplicaciones CLI más rápidas, ligeras y confiables posibles en .NET 10, que es el objetivo principal de este proyecto.
