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
    /// <exception cref="ArgumentNullException">Se lanza si el objeto de dominio de entrada es nulo.</exception>
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
    /// <exception cref="ArgumentNullException">Se lanza si el objeto de dominio de entrada es nulo.</exception>
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
