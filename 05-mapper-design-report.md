# Informe de Diseño de Mapeador de Productos (AOT-Ready)

Fecha: 23 de Febrero de 2026

**Autor:** Gemini, Arquitecto de Software Senior

Este documento detalla la implementación del mapeador para la entidad `Product` dentro del proyecto `UTM_Market`. El `ProductMapper` es una pieza clave en la arquitectura, facilitando la conversión bidireccional entre la entidad de dominio pura (`Product`) y la entidad de persistencia (`ProductoEntity`), manteniendo la separación de capas y asegurando la compatibilidad con `Native AOT` en .NET 10.

## 1. Árbol de Directorios Actualizado

```
C:\geminicode\UTM_Market
├───01-dotnet-10-architect-setup.md
├───02-architectural-setup-report.md
├───02-dotnet-10-entity-architect.md
├───03-dotnet-10-dapper-aot-entities.md
├───03-entity-design-report.md
├───04-dotnet-10-product-mapper-architect.md
├───04-persistence-entity-report.md
├───Program.cs
├───UTM_Market.csproj
├───UTM_Market.sln
├───bin
│   └───Debug
│       └───net10.0
├───db
│   └───scripts
│       ├───01_create_structure_utm_market.sql
│       └───02_seed_data_utm_market.sql
├───obj
│   ├───project.assets.json
│   ├───project.nuget.cache
│   ├───UTM_Market.csproj.nuget.dgspec.json
│   ├───UTM_Market.csproj.nuget.g.props
│   ├───UTM_Market.csproj.nuget.g.targets
│   └───Debug
│       └───net10.0
│           ├───.NETCoreApp,Version=v10.0.AssemblyAttributes.cs
│           ├───UTM_Market.AssemblyInfo.cs
│           ├───UTM_Market.AssemblyInfoInputs.cache
│           ├───UTM_Market.assets.cache
│           ├───UTM_Market.csproj.AssemblyReference.cache
│           ├───UTM_Market.GeneratedMSBuildEditorConfig.editorconfig
│           └───UTM_Market.GlobalUsings.g.cs
├───prompts
│   └───database-integration-prompts
│       ├───01-sql-server-architect.md
│       └───02-sql-seeder-mx-products-2025.md
└───src
    ├───Core
    │   └───Entities
    │       ├───Product.cs
    │       ├───Sale.cs
    │       ├───SaleDetail.cs
    │       └───SaleStatus.cs
    └───Infrastructure
        ├───Mappers
        │   └───ProductMapper.cs
        └───Models
            └───Data
                ├───DetalleVentaEntity.cs
                ├───ProductoEntity.cs
                └───VentaEntity.cs
```

## 2. Código Fuente de `ProductMapper.cs`

La clase `ProductMapper` se ha implementado como una clase estática que proporciona métodos de extensión para la conversión bidireccional, garantizando un código limpio y eficiente.

**`src/Infrastructure/Mappers/ProductMapper.cs`**
```csharp
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
```

## 3. Ejemplo de Uso en un Repositorio

A continuación, un snippet que ilustra cómo se utilizaría el `ProductMapper` dentro de una implementación de repositorio para recuperar y almacenar productos, mostrando la fluidez de los métodos de extensión.

```csharp
// Ejemplo conceptual dentro de un repositorio (Infrastructure layer)

public class ProductRepository // : IProductRepository (interfaz en Core)
{
    // ... dependencia a IDbConnection o similar
    
    public Product GetProductById(int productId)
    {
        // Simulación de lectura desde DB y mapeo manual a ProductoEntity
        var sql = "SELECT ProductoID, Nombre, SKU, Marca, Precio, Stock FROM Producto WHERE ProductoID = @productId";
        // var productoEntity = _dbConnection.QuerySingle<ProductoEntity>(sql, new { productId });
        ProductoEntity productoEntity = new ProductoEntity 
        { 
            ProductoID = productId, Nombre = "Sample", SKU = "SMP-001", Marca = "BrandX", Precio = 100.00m, Stock = 50 
        }; // Placeholder
        
        // Uso del método de extensión ToDomain
        return productoEntity.ToDomain(); 
    }

    public void SaveProduct(Product product)
    {
        // Uso del método de extensión ToEntity para convertir el dominio a entidad de persistencia
        var productoEntity = product.ToEntity(); 

        // Simulación de guardado en DB
        var sql = "INSERT INTO Producto (Nombre, SKU, Marca, Precio, Stock) VALUES (@Nombre, @SKU, @Marca, @Precio, @Stock)";
        // _dbConnection.Execute(sql, productoEntity);
    }
}
```

## 4. Justificación del Uso de 'Extension Blocks' (Métodos de Extensión Estáticos) en C# 14

La utilización de "extension blocks" (implementados como métodos de extensión estáticos en C# actual, y como una característica de lenguaje más integrada en C# 14) en el `ProductMapper` ofrece ventajas significativas en un proyecto optimizado para .NET 10 y Native AOT:

-   **Ergonomía y Legibilidad:** Permiten invocar la lógica de mapeo directamente desde la instancia del objeto (`entity.ToDomain()` o `product.ToEntity()`), lo cual es más intuitivo y legible que pasar el objeto a un método estático (`ProductMapper.ToDomain(entity)`). Esto mejora la experiencia del desarrollador y hace que el código sea más expresivo.

-   **Separación de Responsabilidades:** El mapeo es una preocupación de infraestructura. Al usar métodos de extensión, la lógica de mapeo reside en una clase separada (`ProductMapper`), sin "contaminar" las clases de `Product` o `ProductoEntity` con responsabilidades de conversión. Esto mantiene la pureza de la capa de dominio y la simplicidad de la capa de datos.

-   **Compatibilidad AOT y Rendimiento:** Los métodos de extensión son, en esencia, llamadas a métodos estáticos disfrazadas sintácticamente. Esto significa que **no hay sobrecarga de reflexión o de runtime** asociada a su uso. El compilador AOT los trata como llamadas a métodos estáticos normales, permitiendo un `trimming` completo y eficaz, lo que resulta en binarios más pequeños y un rendimiento de ejecución superior, sin comprometer la velocidad de inicio, crítico para aplicaciones CLI.

-   **Extensibilidad:** Facilita la adición de nuevas funcionalidades de mapeo sin modificar los tipos originales, lo que es una ventaja clave para la extensibilidad y el mantenimiento del código.

Este enfoque equilibra la necesidad de alto rendimiento y compatibilidad AOT con una excelente legibilidad y mantenimiento del código, pilares fundamentales en el diseño de `UTM_Market`.
