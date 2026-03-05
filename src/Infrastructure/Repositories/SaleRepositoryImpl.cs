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
/// Implementación de <see cref="ISaleRepository"/> optimizada para Native AOT.
/// Utiliza ADO.NET puro y mapeo manual para evitar reflexión y problemas N+1.
/// </summary>
/// <param name="dbConnectionFactory">Factoría de conexiones inyectada por constructor primario.</param>
public sealed class SaleRepositoryImpl(IDbConnectionFactory dbConnectionFactory) : ISaleRepository
{
    public async IAsyncEnumerable<Sale> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // Para evitar N+1, recuperamos todo y agrupamos en memoria. 
        // En una app real con millones de registros, usaríamos paginación.
        const string sql = @"
            SELECT v.VentaID, v.Folio, v.FechaVenta, v.TotalArticulos, v.TotalVenta, v.Estatus,
                   dv.DetalleID, dv.ProductoID, dv.PrecioUnitario, dv.Cantidad, dv.TotalDetalle,
                   p.ProductoID, p.Nombre, p.SKU, p.Marca, p.Precio, p.Stock
            FROM Venta v
            LEFT JOIN DetalleVenta dv ON v.VentaID = dv.VentaID
            LEFT JOIN Producto p ON dv.ProductoID = p.ProductoID
            ORDER BY v.VentaID";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        Sale? currentSale = null;
        int? lastVentaId = null;

        while (await reader.ReadAsync(cancellationToken))
        {
            int ventaId = reader.GetInt32(0);

            if (lastVentaId != ventaId)
            {
                if (currentSale != null) yield return currentSale;

                var ventaEntity = MapVentaFromReader(reader);
                currentSale = new Sale(ventaEntity.VentaID, ventaEntity.Folio)
                {
                    SaleDate = ventaEntity.FechaVenta,
                    Status = (SaleStatus)ventaEntity.Estatus
                };
                lastVentaId = ventaId;
            }

            if (!reader.IsDBNull(6)) // Si hay detalle
            {
                var product = MapProductFromReader(reader, offset: 11);
                currentSale!.AddDetail(product.ToDomain(), reader.GetInt32(9));
            }
        }

        if (currentSale != null) yield return currentSale;
    }

    public async Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            SELECT v.VentaID, v.Folio, v.FechaVenta, v.TotalArticulos, v.TotalVenta, v.Estatus,
                   dv.DetalleID, dv.ProductoID, dv.PrecioUnitario, dv.Cantidad, dv.TotalDetalle,
                   p.ProductoID, p.Nombre, p.SKU, p.Marca, p.Precio, p.Stock
            FROM Venta v
            LEFT JOIN DetalleVenta dv ON v.VentaID = dv.VentaID
            LEFT JOIN Producto p ON dv.ProductoID = p.ProductoID
            WHERE v.VentaID = @id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        Sale? sale = null;

        while (await reader.ReadAsync(cancellationToken))
        {
            if (sale == null)
            {
                var ventaEntity = MapVentaFromReader(reader);
                sale = new Sale(ventaEntity.VentaID, ventaEntity.Folio)
                {
                    SaleDate = ventaEntity.FechaVenta,
                    Status = (SaleStatus)ventaEntity.Estatus
                };
            }

            if (!reader.IsDBNull(6))
            {
                var product = MapProductFromReader(reader, offset: 11);
                sale.AddDetail(product.ToDomain(), reader.GetInt32(9));
            }
        }

        return sale;
    }

    public async IAsyncEnumerable<Sale> FindAsync(SaleFilter filter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT v.VentaID, v.Folio, v.FechaVenta, v.TotalArticulos, v.TotalVenta, v.Estatus,
                   dv.DetalleID, dv.ProductoID, dv.PrecioUnitario, dv.Cantidad, dv.TotalDetalle,
                   p.ProductoID, p.Nombre, p.SKU, p.Marca, p.Precio, p.Stock
            FROM Venta v
            LEFT JOIN DetalleVenta dv ON v.VentaID = dv.VentaID
            LEFT JOIN Producto p ON dv.ProductoID = p.ProductoID
            WHERE 1=1";

        using var command = new SqlCommand();
        command.Connection = connection;

        if (filter.StartDate.HasValue)
        {
            sql += " AND v.FechaVenta >= @start";
            command.Parameters.AddWithValue("@start", filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            sql += " AND v.FechaVenta <= @end";
            command.Parameters.AddWithValue("@end", filter.EndDate.Value);
        }

        if (filter.Status.HasValue)
        {
            sql += " AND v.Estatus = @status";
            command.Parameters.AddWithValue("@status", (byte)filter.Status.Value);
        }

        sql += " ORDER BY v.VentaID";
        command.CommandText = sql;

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        Sale? currentSale = null;
        int? lastVentaId = null;

        while (await reader.ReadAsync(cancellationToken))
        {
            int ventaId = reader.GetInt32(0);

            if (lastVentaId != ventaId)
            {
                if (currentSale != null) yield return currentSale;

                var ventaEntity = MapVentaFromReader(reader);
                currentSale = new Sale(ventaEntity.VentaID, ventaEntity.Folio)
                {
                    SaleDate = ventaEntity.FechaVenta,
                    Status = (SaleStatus)ventaEntity.Estatus
                };
                lastVentaId = ventaId;
            }

            if (!reader.IsDBNull(6))
            {
                var product = MapProductFromReader(reader, offset: 11);
                currentSale!.AddDetail(product.ToDomain(), reader.GetInt32(9));
            }
        }

        if (currentSale != null) yield return currentSale;
    }

    public async Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlVenta = @"
                INSERT INTO Venta (Folio, FechaVenta, TotalArticulos, TotalVenta, Estatus)
                VALUES (@folio, @fecha, @totalArt, @totalVenta, @estatus);
                SELECT SCOPE_IDENTITY();";

            var ventaEntity = sale.ToEntity();
            using var cmdVenta = new SqlCommand(sqlVenta, connection, transaction);
            cmdVenta.Parameters.AddWithValue("@folio", ventaEntity.Folio);
            cmdVenta.Parameters.AddWithValue("@fecha", ventaEntity.FechaVenta);
            cmdVenta.Parameters.AddWithValue("@totalArt", ventaEntity.TotalArticulos);
            cmdVenta.Parameters.AddWithValue("@totalVenta", ventaEntity.TotalVenta);
            cmdVenta.Parameters.AddWithValue("@estatus", ventaEntity.Estatus);

            int ventaId = Convert.ToInt32(await cmdVenta.ExecuteScalarAsync(cancellationToken));

            const string sqlDetalle = @"
                INSERT INTO DetalleVenta (VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle)
                VALUES (@ventaId, @prodId, @precio, @cant, @totalDet)";

            foreach (var detail in sale.Details)
            {
                var detailEntity = detail.ToEntity(ventaId);
                using var cmdDetalle = new SqlCommand(sqlDetalle, connection, transaction);
                cmdDetalle.Parameters.AddWithValue("@ventaId", ventaId);
                cmdDetalle.Parameters.AddWithValue("@prodId", detailEntity.ProductoID);
                cmdDetalle.Parameters.AddWithValue("@precio", detailEntity.PrecioUnitario);
                cmdDetalle.Parameters.AddWithValue("@cant", detailEntity.Cantidad);
                cmdDetalle.Parameters.AddWithValue("@totalDet", detailEntity.TotalDetalle);
                await cmdDetalle.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            
            // Retornamos el objeto con el ID asignado (usando reflexión interna de C# para simular rehidratación si fuera necesario, 
            // pero aquí creamos una nueva instancia para cumplir con la inmutabilidad de SaleID si se desea, 
            // o simplemente retornamos el mismo si SaleID fuera mutable. Según la definición dada, SaleID es readonly).
            return await GetByIdAsync(ventaId, cancellationToken) ?? throw new InvalidOperationException("Error al recuperar la venta recién creada.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)dbConnectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlUpdateVenta = @"
                UPDATE Venta 
                SET Folio = @folio, FechaVenta = @fecha, TotalArticulos = @totalArt, TotalVenta = @totalVenta, Estatus = @estatus
                WHERE VentaID = @id";

            var ventaEntity = sale.ToEntity();
            using var cmdVenta = new SqlCommand(sqlUpdateVenta, connection, transaction);
            cmdVenta.Parameters.AddWithValue("@id", ventaEntity.VentaID);
            cmdVenta.Parameters.AddWithValue("@folio", ventaEntity.Folio);
            cmdVenta.Parameters.AddWithValue("@fecha", ventaEntity.FechaVenta);
            cmdVenta.Parameters.AddWithValue("@totalArt", ventaEntity.TotalArticulos);
            cmdVenta.Parameters.AddWithValue("@totalVenta", ventaEntity.TotalVenta);
            cmdVenta.Parameters.AddWithValue("@estatus", ventaEntity.Estatus);
            await cmdVenta.ExecuteNonQueryAsync(cancellationToken);

            // Para simplificar, borramos y re-insertamos detalles (estrategia común en repositorios de agregados)
            const string sqlDeleteDetails = "DELETE FROM DetalleVenta WHERE VentaID = @ventaId";
            using var cmdDel = new SqlCommand(sqlDeleteDetails, connection, transaction);
            cmdDel.Parameters.AddWithValue("@ventaId", sale.SaleID);
            await cmdDel.ExecuteNonQueryAsync(cancellationToken);

            const string sqlDetalle = @"
                INSERT INTO DetalleVenta (VentaID, ProductoID, PrecioUnitario, Cantidad, TotalDetalle)
                VALUES (@ventaId, @prodId, @precio, @cant, @totalDet)";

            foreach (var detail in sale.Details)
            {
                var detailEntity = detail.ToEntity(sale.SaleID);
                using var cmdDetalle = new SqlCommand(sqlDetalle, connection, transaction);
                cmdDetalle.Parameters.AddWithValue("@ventaId", sale.SaleID);
                cmdDetalle.Parameters.AddWithValue("@prodId", detailEntity.ProductoID);
                cmdDetalle.Parameters.AddWithValue("@precio", detailEntity.PrecioUnitario);
                cmdDetalle.Parameters.AddWithValue("@cant", detailEntity.Cantidad);
                cmdDetalle.Parameters.AddWithValue("@totalDet", detailEntity.TotalDetalle);
                await cmdDetalle.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static VentaEntity MapVentaFromReader(SqlDataReader reader)
    {
        return new VentaEntity
        {
            VentaID = reader.GetInt32(0),
            Folio = reader.GetString(1),
            FechaVenta = reader.GetDateTime(2),
            TotalArticulos = reader.GetInt32(3),
            TotalVenta = reader.GetDecimal(4),
            Estatus = reader.GetByte(5)
        };
    }

    private static ProductoEntity MapProductFromReader(SqlDataReader reader, int offset)
    {
        return new ProductoEntity
        {
            ProductoID = reader.GetInt32(offset + 0),
            Nombre = reader.GetString(offset + 1),
            SKU = reader.GetString(offset + 2),
            Marca = reader.IsDBNull(offset + 3) ? null : reader.GetString(offset + 3),
            Precio = reader.GetDecimal(offset + 4),
            Stock = reader.GetInt32(offset + 5)
        };
    }
}