using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using UTM_Market.Core.Abstractions;
using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;
using UTM_Market.Core.Repositories;
using UTM_Market.Infrastructure.Mappers;
using UTM_Market.Infrastructure.Models.Data;

namespace UTM_Market.Infrastructure.Repositories;

/// <summary>
/// Implementación de <see cref="IProductRepository"/> optimizada para Native AOT.
/// Utiliza mapeo manual con SqlDataReader para evitar la reflexión de Dapper.
/// </summary>
/// <param name="dbConnectionFactory">Factoría de conexiones inyectada por constructor primario.</param>
public sealed class ProductRepositoryImpl(IDbConnectionFactory dbConnectionFactory) : IProductRepository
{
    public async IAsyncEnumerable<Product> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = "SELECT ProductoID, Nombre, SKU, Marca, Precio, Stock FROM Producto";
        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapFromReader(reader).ToDomain();
        }
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = "SELECT ProductoID, Nombre, SKU, Marca, Precio, Stock FROM Producto WHERE ProductoID = @id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapFromReader(reader).ToDomain();
        }

        return null;
    }

    public async IAsyncEnumerable<Product> FindAsync(ProductFilter filter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT ProductoID, Nombre, SKU, Marca, Precio, Stock FROM Producto WHERE 1=1";
        using var command = new SqlCommand();
        command.Connection = connection;

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            sql += " AND Nombre LIKE @name";
            command.Parameters.AddWithValue("@name", $"%{filter.Name}%");
        }

        if (!string.IsNullOrWhiteSpace(filter.Sku))
        {
            sql += " AND SKU = @sku";
            command.Parameters.AddWithValue("@sku", filter.Sku);
        }

        if (filter.MinPrice.HasValue)
        {
            sql += " AND Precio >= @minPrice";
            command.Parameters.AddWithValue("@minPrice", filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            sql += " AND Precio <= @maxPrice";
            command.Parameters.AddWithValue("@maxPrice", filter.MaxPrice.Value);
        }

        command.CommandText = sql;
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapFromReader(reader).ToDomain();
        }
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            INSERT INTO Producto (Nombre, SKU, Marca, Precio, Stock)
            VALUES (@nombre, @sku, @marca, @precio, @stock);
            SELECT SCOPE_IDENTITY();";

        var entity = product.ToEntity();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nombre", entity.Nombre);
        command.Parameters.AddWithValue("@sku", entity.SKU);
        command.Parameters.AddWithValue("@marca", (object?)entity.Marca ?? DBNull.Value);
        command.Parameters.AddWithValue("@precio", entity.Precio);
        command.Parameters.AddWithValue("@stock", entity.Stock);

        var id = await command.ExecuteScalarAsync(cancellationToken);
        // Actualizamos el ID en el objeto de dominio (si es necesario, aunque el contrato es Task)
        // Como el ID es de solo lectura en el dominio según la definición dada, 
        // en un escenario real rehidrataríamos el objeto o lo devolveríamos.
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            UPDATE Producto 
            SET Nombre = @nombre, SKU = @sku, Marca = @marca, Precio = @precio, Stock = @stock
            WHERE ProductoID = @id";

        var entity = product.ToEntity();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", entity.ProductoID);
        command.Parameters.AddWithValue("@nombre", entity.Nombre);
        command.Parameters.AddWithValue("@sku", entity.SKU);
        command.Parameters.AddWithValue("@marca", (object?)entity.Marca ?? DBNull.Value);
        command.Parameters.AddWithValue("@precio", entity.Precio);
        command.Parameters.AddWithValue("@stock", entity.Stock);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateStockAsync(int productId, int newStock, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = "UPDATE Producto SET Stock = @stock WHERE ProductoID = @id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", productId);
        command.Parameters.AddWithValue("@stock", newStock);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Mapeo manual de SqlDataReader a ProductoEntity.
    /// Esta es la clave para la compatibilidad con Native AOT ya que no utiliza reflexión.
    /// </summary>
    private static ProductoEntity MapFromReader(SqlDataReader reader)
    {
        return new ProductoEntity
        {
            ProductoID = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            SKU = reader.GetString(2),
            Marca = reader.IsDBNull(3) ? null : reader.GetString(3),
            Precio = reader.GetDecimal(4),
            Stock = reader.GetInt32(5)
        };
    }
}