using UTM_Market.Core.Entities;
using UTM_Market.Core.Repositories;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Implementation for registering a new customer.
/// </summary>
public sealed class RegisterCustomerUseCaseImpl(ICustomerRepository customerRepository) : IRegisterCustomerUseCase
{
    public async Task<Customer> ExecuteAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        // Validación de negocio: El email debe ser único.
        var existingCustomer = await customerRepository.GetByEmailAsync(customer.Email, cancellationToken);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"El correo electrónico '{customer.Email}' ya está registrado.");
        }

        return await customerRepository.AddAsync(customer, cancellationToken);
    }
}
