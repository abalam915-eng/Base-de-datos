using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for searching a customer by email.
/// </summary>
public sealed class GetCustomerByEmailUseCaseImpl(ICustomerRepository customerRepository) : IGetCustomerByEmailUseCase
{
    public Task<Customer?> ExecuteAsync(string email, CancellationToken cancellationToken = default)
    {
        return customerRepository.GetByEmailAsync(email, cancellationToken);
    }
}
