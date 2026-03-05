using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for retrieving all customers.
/// </summary>
public sealed class GetAllCustomersUseCaseImpl(ICustomerRepository customerRepository) : IGetAllCustomersUseCase
{
    public IAsyncEnumerable<Customer> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return customerRepository.GetAllAsync(cancellationToken);
    }
}
