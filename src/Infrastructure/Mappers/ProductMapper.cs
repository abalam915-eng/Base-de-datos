using UTM_Market.Core.Entities;
using UTM_Market.Infrastructure.Models.Data;

namespace UTM_Market.Infrastructure.Mappers;

/// <summary>
/// Clase estática que proporciona métodos de extensión para mapear
/// entre la entidad de dominio 'Product' y la entidad de persistencia 'ProductoEntity'.
/// Implementa los "extension blocks" de C# 14 (conceptualmente, como métodos de extensión estáticos)
/// para mejorar la ergonomía del código y la compatibilidad con Native AOT.
/// </summary>
public static class ProductMapper
{
    /// <summary>
    /// Convierte un objeto <see cref="ProductoEntity"/> de la capa de infraestructura
    /// a un objeto de dominio <see cref="Product"/>.
    /// </summary>
    /// <param name="entity">La entidad de persistencia a convertir.</param>
    /// <returns>Un objeto de dominio <see cref="Product"/>.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si la entidad de entrada es nula.</exception>
    public static Product ToDomain(this ProductoEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity); // Defensa de nulidad moderna de C#

        // Uso del constructor primario de la entidad de dominio.
        var product = new Product(
            entity.ProductoID,
            entity.Nombre, // Mapeo de Nombre a Name
            entity.SKU,
            entity.Marca ?? string.Empty // Mapeo de Marca a Brand (manejo de nulos)
        );

        // Asignación de propiedades con setters que contienen lógica de validación.
        // Las validaciones con 'field' se activarán aquí.
        product.Price = entity.Precio;
        product.Stock = entity.Stock;

        return product;
    }

    /// <summary>
    /// Convierte un objeto de dominio <see cref="Product"/>
    /// a un objeto de persistencia <see cref="ProductoEntity"/>.
    /// </summary>
    /// <param name="domain">El objeto de dominio a convertir.</param>
    /// <returns>Un objeto <see cref="ProductoEntity"/> de la capa de infraestructura.</returns>
    /// <exception cref="ArgumentNullException">Se lanza si el objeto de dominio de entrada es nulo.</exception>
    public static ProductoEntity ToEntity(this Product domain)
    {
        ArgumentNullException.ThrowIfNull(domain); // Defensa de nulidad moderna de C#

        return new ProductoEntity
        {
            ProductoID = domain.ProductID,
            Nombre = domain.Name, // Mapeo de Name a Nombre
            SKU = domain.SKU,
            Marca = domain.Brand, // Mapeo de Brand a Marca
            Precio = domain.Price,
            Stock = domain.Stock
        };
    }
}
