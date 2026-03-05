using UTM_Market.Core.Entities;

namespace UTM_Market.Core.Repositories;

/// <summary>
/// Define el contrato de persistencia para la entidad <see cref="Customer"/>.
/// Diseñado para ser agnóstico a la infraestructura y optimizado para Native AOT.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Obtiene un cliente por su dirección de correo electrónico única.
    /// </summary>
    /// <param name="email">El correo electrónico a buscar.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>El <see cref="Customer"/> encontrado o null si no existe.</returns>
    /// <exception cref="ArgumentException">Si el email es nulo o vacío.</exception>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un nuevo cliente en el sistema.
    /// </summary>
    /// <param name="customer">La entidad de dominio con los datos del cliente.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>La instancia de <see cref="Customer"/> persistida, incluyendo su ID generado.</returns>
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Proporciona un flujo asíncrono de todos los clientes registrados.
    /// Optimizado para lectura en streaming (IAsyncEnumerable) evitando buffering en memoria.
    /// </summary>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Un flujo de objetos <see cref="Customer"/>.</returns>
    IAsyncEnumerable<Customer> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un cliente por su identificador único.
    /// </summary>
    /// <param name="id">El ID del cliente.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>El <see cref="Customer"/> encontrado o null.</returns>
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la información de un cliente existente.
    /// </summary>
    /// <param name="customer">La entidad con los datos actualizados.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelación.</param>
    /// <returns>Tarea que representa la operación.</returns>
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
}
