// src/Core/Abstractions/IDbConnectionFactory.cs
using System.Data;

namespace UTM_Market.Core.Abstractions;

/// <summary>
/// Defines a factory for creating database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and returns a new database connection.
    /// </summary>
    /// <returns>A new instance of <see cref="IDbConnection"/>.</returns>
    IDbConnection CreateConnection();
}
