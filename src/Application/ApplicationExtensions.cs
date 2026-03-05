using Microsoft.Extensions.DependencyInjection;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Extension block for IServiceCollection to register Application Use Cases.
/// </summary>
public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Product Use Cases
        services.AddScoped<IGetAllProductsUseCase, GetAllProductsUseCaseImpl>();
        services.AddScoped<IGetProductByIdUseCase, GetProductByIdUseCaseImpl>();
        services.AddScoped<ISearchProductsUseCase, SearchProductsUseCaseImpl>();
        services.AddScoped<ICreateProductUseCase, CreateProductUseCaseImpl>();
        services.AddScoped<IUpdateProductUseCase, UpdateProductUseCaseImpl>();
        services.AddScoped<IUpdateProductStockUseCase, UpdateProductStockUseCaseImpl>();
        services.AddScoped<ILowStockAlertUseCase, LowStockAlertUseCaseImpl>();

        // Sale Use Cases
        services.AddScoped<IFetchAllSalesUseCase, FetchAllSalesUseCaseImpl>();
        services.AddScoped<IFetchSaleByIdUseCase, FetchSaleByIdUseCaseImpl>();
        services.AddScoped<IFetchSalesByFilterUseCase, FetchSalesByFilterUseCaseImpl>();
        services.AddScoped<ICreateSaleUseCase, CreateSaleUseCaseImpl>();
        services.AddScoped<IUpdateSaleStatusUseCase, UpdateSaleStatusUseCaseImpl>();

        // Customer Use Cases
        services.AddScoped<IGetAllCustomersUseCase, GetAllCustomersUseCaseImpl>();
        services.AddScoped<IGetCustomerByEmailUseCase, GetCustomerByEmailUseCaseImpl>();
        services.AddScoped<IRegisterCustomerUseCase, RegisterCustomerUseCaseImpl>();

        return services;
    }
}