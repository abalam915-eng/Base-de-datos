using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UTM_Market.Core.Abstractions;
using UTM_Market.Core.Repositories;
using UTM_Market.Infrastructure.Configuration;
using UTM_Market.Infrastructure.Data;
using UTM_Market.Infrastructure.Repositories;

namespace UTM_Market.Infrastructure.Extensions;

/// <summary>
/// Static class to register persistence services.
/// </summary>
public static class PersistenceExtensions
{
    /// <summary>
    /// Registers persistence services.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IProductRepository, ProductRepositoryImpl>();
        services.AddScoped<ISaleRepository, SaleRepositoryImpl>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
