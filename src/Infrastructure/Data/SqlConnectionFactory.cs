// src/Infrastructure/Data/SqlConnectionFactory.cs
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using UTM_Market.Core.Abstractions;
using UTM_Market.Infrastructure.Configuration; // Assuming DatabaseOptions is here

namespace UTM_Market.Infrastructure.Data;

/// <summary>
/// Implementation of <see cref="IDbConnectionFactory"/> for SQL Server.
/// </summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly IOptions<DatabaseOptions> _databaseOptions;

    public SqlConnectionFactory(IOptions<DatabaseOptions> databaseOptions)
    {
        _databaseOptions = databaseOptions 
            ?? throw new ArgumentNullException(nameof(databaseOptions));
        
        if (string.IsNullOrWhiteSpace(_databaseOptions.Value?.DefaultConnection))
        {
            throw new ArgumentException("La cadena de conexión en DatabaseOptions no puede ser nula o estar vacía.", nameof(databaseOptions));
        }
    }

    /// <summary>
    /// Creates a new <see cref="SqlConnection"/>.
    /// </summary>
    /// <returns>A new <see cref="SqlConnection"/>.</returns>
    public IDbConnection CreateConnection() => new SqlConnection(_databaseOptions.Value.DefaultConnection);
}
