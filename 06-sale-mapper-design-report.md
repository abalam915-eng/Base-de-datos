# Informe de Diseño de Mapeador de Ventas (Deep Mapping, AOT-Ready)

Fecha: 23 de Febrero de 2026

**Autor:** Gemini, Arquitecto de Software Senior

Este documento detalla la implementación del mapeador para la entidad `Sale` y sus `SaleDetail` asociados dentro del proyecto `UTM_Market`. El `SaleMapper` es fundamental para orquestar la transformación profunda (deep mapping) entre el agregado de dominio (`Sale` con sus `SaleDetail`) y las entidades de persistencia (`VentaEntity` con sus `DetalleVentaEntity`), garantizando la compatibilidad con `Native AOT` en .NET 10 y la preservación de la lógica de negocio.

## 1. Árbol de Directorios de la Capa de Infraestructura

```
C:\geminicode\UTM_Market\src\Infrastructure
├───Mappers
│   ├───ProductMapper.cs
│   └───SaleMapper.cs
└───Models
    └───Data
        ├───DetalleVentaEntity.cs
        ├───ProductoEntity.cs
        └───VentaEntity.cs
```

## 2. Código Fuente de `SaleMapper.cs`

La clase `SaleMapper` se ha implementado como una clase estática que proporciona métodos de extensión para la conversión bidireccional, incluyendo el manejo de colecciones y dependencias.

**`src/Infrastructure/Mappers/SaleMapper.cs`**
```csharp
using UTM_Market.Core.Entities;
using UTM_Market.Infrastructure.Models.Data;
using System.Linq;
using System.Collections.Generic;

namespace UTM_Market.Infrastructure.Mappers;

/// <summary>
/// Clase estática que proporciona métodos de extensión para mapear
/// entre las entidades de dominio 'Sale'/'SaleDetail' y las entidades de persistencia
/// 'VentaEntity'/'DetalleVentaEntity'.
/// Implementa mapeo profundo y está optimizada para compatibilidad con Native AOT.
/// </summary>
public static class SaleMapper
{
    // ===============================================================
    // Mapeo de DetalleVentaEntity <-> SaleDetail
    // ===============================================================

    /// <summary>
    /// Convierte un <see cref="DetalleVentaEntity"/> a un <see cref="SaleDetail"/> de dominio.
    /// Este método requiere que el <see cref="Product"/> de dominio asociado ya haya sido resuelto.
    /// </summary>
    /// <param name="entity">La entidad de persistencia del detalle de venta.</param>
    /// <param name="product">El producto de dominio asociado a este detalle.</param>
    /// <returns>Un objeto de dominio <see cref="SaleDetail"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si alguna entidad de entrada es nula.</exception>
    public static SaleDetail ToDomain(this DetalleVentaEntity entity, Product product)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(product);

        // SaleDetail's constructor takes Product and Quantity. UnitPrice is derived from Product.Price.
        // We pass the already mapped domain Product to ensure the domain integrity.
        return new SaleDetail(product, entity.Cantidad);
    }

    /// <summary>
    /// Convierte un <see cref="SaleDetail"/> de dominio a un <see cref="DetalleVentaEntity"/> de persistencia.
    /// Requiere el VentaID al que pertenece el detalle.
    /// </summary>
    /// <param name="domain">El objeto de dominio <see cref="SaleDetail"/>.</param>
    /// <param name="ventaId">El ID de la venta a la que pertenece este detalle.</param>
    /// <returns>Un objeto de persistencia <see cref="DetalleVentaEntity"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el objeto de dominio de entrada es nula.</exception>
    public static DetalleVentaEntity ToEntity(this SaleDetail domain, int ventaId)
    {
        ArgumentNullException.ThrowIfNull(domain);

        return new DetalleVentaEntity
        {
            DetalleID = 0, // Generalmente se auto-genera en DB, o se asigna en un contexto de creación
            VentaID = ventaId,
            ProductoID = domain.Product.ProductID,
            PrecioUnitario = domain.UnitPrice,
            Cantidad = domain.Quantity,
            TotalDetalle = domain.TotalDetail
        };
    }

    // ===============================================================
    // Mapeo de VentaEntity <-> Sale
    // ===============================================================

    /// <summary>
    /// Convierte una <see cref="VentaEntity"/> y su colección de <see cref="DetalleVentaEntity"/>
    /// a un objeto de dominio <see cref="Sale"/>.
    /// Este método orquesta el mapeo profundo, requiriendo una función para resolver
    /// los productos de dominio a partir de sus IDs.
    /// </summary>
    /// <param name="ventaEntity">La entidad de persistencia de la venta.</param>
    /// <param name="detalleEntities">La colección de entidades de persistencia de detalles de venta.</param>
    /// <param name="getProductDomainById">Una función para obtener un <see cref="Product"/> de dominio dado su ID.</param>
    /// <returns>Un objeto de dominio <see cref="Sale"/> completamente mapeado.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si alguna entidad o función de entrada es nula.</exception>
    public static Sale ToDomain(this VentaEntity ventaEntity, IEnumerable<DetalleVentaEntity> detalleEntities, Func<int, Product> getProductDomainById)
    {
        ArgumentNullException.ThrowIfNull(ventaEntity);
        ArgumentNullException.ThrowIfNull(detalleEntities);
        ArgumentNullException.ThrowIfNull(getProductDomainById);

        var sale = new Sale(ventaEntity.VentaID, ventaEntity.Folio)
        {
            SaleDate = ventaEntity.FechaVenta,
            Status = (SaleStatus)ventaEntity.Estatus // Mapeo de byte (TINYINT) a enum
        };

        // Mapeo de detalles y adición al objeto de dominio Sale
        foreach (var detalleEntity in detalleEntities)
        {
            var product = getProductDomainById(detalleEntity.ProductoID);
            var saleDetail = detalleEntity.ToDomain(product);
            
            // AddDetail gestiona la adición o actualización de detalles, preservando la lógica de negocio del dominio.
            sale.AddDetail(saleDetail.Product, saleDetail.Quantity); 
        }

        return sale;
    }

    /// <summary>
    /// Convierte un objeto de dominio <see cref="Sale"/> a una <see cref="VentaEntity"/>
    /// de persistencia. Los detalles deben ser mapeados por separado si se requiere una colección.
    /// </summary>
    /// <param name="domain">El objeto de dominio <see cref="Sale"/>.</param>
    /// <returns>Un objeto de persistencia <see cref="VentaEntity"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el objeto de dominio de entrada es nula.</exception>
    public static VentaEntity ToEntity(this Sale domain)
    {
        ArgumentNullException.ThrowIfNull(domain);
        
        return new VentaEntity
        {
            VentaID = domain.SaleID,
            Folio = domain.Folio,
            FechaVenta = domain.SaleDate,
            TotalArticulos = domain.TotalItems, // Se copian los valores calculados del dominio
            TotalVenta = domain.TotalSale,       // Se copian los valores calculados del dominio
            Estatus = (byte)domain.Status       // Mapeo de enum a byte (TINYINT)
        };
    }
    
    /// <summary>
    /// Convierte una colección de <see cref="SaleDetail"/> a una colección de <see cref="DetalleVentaEntity"/>.
    /// </summary>
    /// <param name="domainDetails">La colección de detalles de dominio.</param>
    /// <param name="ventaId">El ID de la venta a la que pertenecen estos detalles.</param>
    /// <returns>Una lista de <see cref="DetalleVentaEntity"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la colección de entrada es nula.</exception>
    public static List<DetalleVentaEntity> ToEntities(this IEnumerable<SaleDetail> domainDetails, int ventaId)
    {
        ArgumentNullException.ThrowIfNull(domainDetails);
        return domainDetails.Select(d => d.ToEntity(ventaId)).ToList();
    }
}
```

## 3. Ejemplo de Uso en un Repositorio

A continuación, un snippet que ilustra cómo se utilizaría el `SaleMapper` dentro de una implementación de repositorio para recuperar una `Sale` con sus detalles desde la base de datos (usando Dapper) y convertirla a un objeto de dominio.

```csharp
using UTM_Market.Core.Entities;
using UTM_Market.Infrastructure.Models.Data;
using UTM_Market.Infrastructure.Mappers;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Linq;

// Asumimos que ProductMapper existe y funciona correctamente.

public class SaleRepository // : ISaleRepository (interfaz en Core)
{
    private readonly string _connectionString;

    public SaleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Método para resolver Products (simulado para este ejemplo)
    private Product GetProductDomainById(int productId)
    {
        // En una implementación real, esto llamaría a ProductRepository.GetById()
        // o cargaría el ProductoEntity y lo mapearía.
        // Aquí se devuelve un Product dummy para fines de demostración del mapeo.
        return new Product(productId, $"Product_{productId}", $"SKU-{productId}", "BrandA")
        {
            Price = 10.0m + productId,
            Stock = 100 - productId
        };
    }

    public Sale GetSaleById(int saleId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        // Cargar VentaEntity
        var ventaEntity = connection.QuerySingleOrDefault<VentaEntity>(
            "SELECT VentaID, Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus FROM Venta WHERE VentaID = @SaleID",
            new { SaleID = saleId });

        if (ventaEntity == null)
        {
            return null;
        }

        // Cargar DetalleVentaEntity
        var detalleEntities = connection.Query<DetalleVentaEntity>(
            "SELECT DetalleID, VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle FROM DetalleVenta WHERE VentaID = @VentaID",
            new { VentaID = saleId }).ToList();

        // Usar el mapeador para convertir a Sale de dominio
        return ventaEntity.ToDomain(detalleEntities, GetProductDomainById);
    }

    public void SaveSale(Sale sale)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Mapear Sale a VentaEntity
            var ventaEntity = sale.ToEntity();

            // Insertar o actualizar VentaEntity
            if (ventaEntity.VentaID == 0) // Nueva venta
            {
                ventaEntity.VentaID = connection.ExecuteScalar<int>(
                    "INSERT INTO Venta (Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus) OUTPUT INSERTED.VentaID VALUES (@Folio, @FechaVenta, @TotalArticulos, @TotalVenta, @Estatus)",
                    ventaEntity, transaction);
            }
            else // Actualizar venta existente (simplificado)
            {
                connection.Execute(
                    "UPDATE Venta SET Folio = @Folio, FechaVenta = @FechaVenta, TotalArticulos = @TotalArticulos, TotalVenta = @TotalVenta, Estatus = @Estatus WHERE VentaID = @VentaID",
                    ventaEntity, transaction);
                
                // Borrar detalles antiguos y insertar nuevos (simplificado para este ejemplo)
                connection.Execute("DELETE FROM DetalleVenta WHERE VentaID = @VentaID", new { VentaID = ventaEntity.VentaID }, transaction);
            }

            // Mapear y guardar detalles
            var detalleEntities = sale.Details.ToEntities(ventaEntity.VentaID);
            foreach (var detalleEntity in detalleEntities)
            {
                connection.Execute(
                    "INSERT INTO DetalleVenta (VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle) VALUES (@VentaID, @ProductoID, @PrecioUnitario, @Cantidad, @TotalDetalle)",
                    detalleEntity, transaction);
            }

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

## 4. Nota de Arquitectura: Beneficios de C# 14 Extension Members para Mapeo Profundo AOT

En el contexto de .NET 10, C# 14 y `Native AOT`, el diseño de `SaleMapper` como una clase estática con métodos de extensión (conceptualmente los "extension blocks" de C# 14) ofrece ventajas cruciales para el mapeo profundo:

-   **Claridad y Consistencia en la Carga Agregada:** El patrón de métodos de extensión permite que la lógica de mapeo encapsule la complejidad de cargar y ensamblar un agregado completo (`Sale` y sus `SaleDetail`). La firma `ventaEntity.ToDomain(detalleEntities, getProductDomainById)` comunica claramente las dependencias necesarias para reconstruir el objeto de dominio de forma consistente, delegando la resolución de `Product` a un proveedor externo, como el repositorio de `Product`.

-   **Preservación de la Invariante del Dominio:** Al utilizar `sale.AddDetail()` en el proceso de mapeo (`ToDomain`), se respeta la lógica de negocio definida en el agregado de dominio. Esto asegura que cualquier validación o comportamiento asociado a la adición de detalles se ejecute, en lugar de simplemente asignar una lista directamente, lo cual podría romper las invariantes del dominio.

-   **Optimización AOT sin Reflexión:** Los métodos de extensión son, en última instancia, llamadas a métodos estáticos. Esto significa que el compilador `Native AOT` puede optimizar completamente el código, realizando un `trimming` agresivo y generando un binario altamente eficiente. No hay sobrecarga de reflexión, lo que garantiza el máximo rendimiento y un tamaño de aplicación reducido, elementos críticos para aplicaciones CLI de alto rendimiento.

-   **Manejo Elegante de Dependencias:** El uso de delegados (`Func<int, Product>`) para resolver dependencias (`Product`) es una forma limpia y AOT-compatible de inyectar el comportamiento necesario sin introducir dependencias directas entre los mapeadores o violar el principio de inversión de dependencias.

Este enfoque logra un equilibrio óptimo entre la expresividad del código, la mantenibilidad y los requisitos estrictos de rendimiento y compatibilidad de `Native AOT` para aplicaciones transaccionales.
