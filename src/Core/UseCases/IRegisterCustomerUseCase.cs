using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for registering a new customer in the loyalty system.
/// </summary>
public interface IRegisterCustomerUseCase
{
    /// <summary>
    /// Executes the registration of a new customer.
    /// </summary>
    /// <param name="customer">The customer domain entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registered customer with its generated ID.</returns>
    Task<Customer> ExecuteAsync(Customer customer, CancellationToken cancellationToken = default);
}
