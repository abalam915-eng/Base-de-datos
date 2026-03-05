using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using UTM_Market.Core.Abstractions;
using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Infrastructure.Mappers;
using UTM_Market.Infrastructure.Models.Data;

namespace UTM_Market.Infrastructure.Repositories;

/// <summary>
/// Implementación de <see cref="ICustomerRepository"/> optimizada para Native AOT.
/// Utiliza ADO.NET puro (SqlDataReader) y mapeo manual para ser 100% "Reflection-Free".
/// </summary>
/// <param name="connectionFactory">Fábrica de conexiones inyectada mediante Primary Constructor.</param>
public sealed class CustomerRepository(IDbConnectionFactory connectionFactory) : ICustomerRepository
{
    private const string SelectColumns = "[ClienteID], [NombreCompleto], [Email], [EsActivo], [FechaRegistro]";

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = $"SELECT {SelectColumns} FROM [dbo].[Clientes] WHERE [Email] = @Email";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapToEntity(reader).ToDomain();
        }

        return null;
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            INSERT INTO [dbo].[Clientes] ([NombreCompleto], [Email], [EsActivo], [FechaRegistro])
            OUTPUT INSERTED.[ClienteID]
            VALUES (@NombreCompleto, @Email, @EsActivo, @FechaRegistro)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@NombreCompleto", customer.NombreCompleto);
        command.Parameters.AddWithValue("@Email", customer.Email);
        command.Parameters.AddWithValue("@EsActivo", customer.EsActivo);
        command.Parameters.AddWithValue("@FechaRegistro", customer.FechaRegistro);

        var generatedId = (int)await command.ExecuteScalarAsync(cancellationToken);

        return await GetByIdAsync(generatedId, cancellationToken) 
               ?? throw new InvalidOperationException("Error crítico: No se pudo recuperar el cliente tras la inserción.");
    }

    public async IAsyncEnumerable<Customer> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = $"SELECT {SelectColumns} FROM [dbo].[Clientes] ORDER BY [NombreCompleto]";
        using var command = new SqlCommand(sql, connection);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        int idPos = reader.GetOrdinal("ClienteID");
        int namePos = reader.GetOrdinal("NombreCompleto");
        int emailPos = reader.GetOrdinal("Email");
        int activePos = reader.GetOrdinal("EsActivo");
        int datePos = reader.GetOrdinal("FechaRegistro");

        while (await reader.ReadAsync(cancellationToken))
        {
            var entity = new CustomerEntity(
                clienteID: reader.GetInt32(idPos),
                nombreCompleto: reader.GetString(namePos),
                email: reader.GetString(emailPos),
                esActivo: reader.GetBoolean(activePos),
                fechaRegistro: reader.GetDateTime(datePos)
            );

            yield return entity.ToDomain();
        }
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = $"SELECT {SelectColumns} FROM [dbo].[Clientes] WHERE [ClienteID] = @Id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapToEntity(reader).ToDomain();
        }

        return null;
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        using var connection = (SqlConnection)connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            UPDATE [dbo].[Clientes]
            SET [NombreCompleto] = @NombreCompleto, [Email] = @Email, [EsActivo] = @EsActivo
            WHERE [ClienteID] = @ClienteID";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ClienteID", customer.ClienteID);
        command.Parameters.AddWithValue("@NombreCompleto", customer.NombreCompleto);
        command.Parameters.AddWithValue("@Email", customer.Email);
        command.Parameters.AddWithValue("@EsActivo", customer.EsActivo);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static CustomerEntity MapToEntity(SqlDataReader reader)
    {
        return new CustomerEntity(
            clienteID: reader.GetInt32(reader.GetOrdinal("ClienteID")),
            nombreCompleto: reader.GetString(reader.GetOrdinal("NombreCompleto")),
            email: reader.GetString(reader.GetOrdinal("Email")),
            esActivo: reader.GetBoolean(reader.GetOrdinal("EsActivo")),
            fechaRegistro: reader.GetDateTime(reader.GetOrdinal("FechaRegistro"))
        );
    }
}
