using UTM_Market.Core.Entities;
using UTM_Market.Infrastructure.Models.Data;

namespace UTM_Market.Infrastructure.Mappers;

/// <summary>
/// Mapeador de alto rendimiento para la entidad Customer.
/// Diseñado para ser 100% compatible con Native AOT (Zero Reflection).
/// </summary>
public static class CustomerMapper
{
    /// <summary>
    /// Convierte un CustomerEntity (Infraestructura) a Customer (Dominio).
    /// </summary>
    public static Customer ToDomain(this CustomerEntity entity)
    {
        return new Customer(entity.ClienteID, entity.NombreCompleto, entity.FechaRegistro)
        {
            EsActivo = entity.EsActivo,
            Email = entity.Email
        };
    }

    /// <summary>
    /// Convierte un Customer (Dominio) a CustomerEntity (Infraestructura).
    /// </summary>
    public static CustomerEntity ToEntity(this Customer domain)
    {
        return new CustomerEntity(
            domain.ClienteID,
            domain.NombreCompleto,
            domain.Email,
            domain.EsActivo,
            domain.FechaRegistro
        );
    }

    /// <summary>
    /// Método auxiliar para mapeo de colecciones optimizado para .NET 10.
    /// </summary>
    public static IEnumerable<Customer> ToDomainList(this IEnumerable<CustomerEntity> entities)
    {
        foreach (var entity in entities)
        {
            yield return entity.ToDomain();
        }
    }
}
