using UTM_Market.Core.Entities;

namespace UTM_Market.Core.UseCases;

/// <summary>
/// Use Case for retrieving a customer by their unique email.
/// </summary>
public interface IGetCustomerByEmailUseCase
{
    /// <summary>
    /// Executes the search for a customer by email.
    /// </summary>
    /// <param name="email">The customer's email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer if found, otherwise null.</returns>
    Task<Customer?> ExecuteAsync(string email, CancellationToken cancellationToken = default);
}
