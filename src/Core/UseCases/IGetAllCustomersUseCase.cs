using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for retrieving all customers in the system.
/// </summary>
public interface IGetAllCustomersUseCase
{
    /// <summary>
    /// Executes the use case to retrieve a stream of all customers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async stream of domain customers.</returns>
    IAsyncEnumerable<Customer> ExecuteAsync(CancellationToken cancellationToken = default);
}
