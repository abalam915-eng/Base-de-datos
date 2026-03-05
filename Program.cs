using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UTM_Market.Application;
using UTM_Market.Core.UseCases;
using UTM_Market.Infrastructure.Extensions;

// .NET 10 modern standards: Use HostApplicationBuilder
var builder = Host.CreateApplicationBuilder(args);

// Ensure User Secrets are loaded in Development environment
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

// Logging Configuration
builder.Services.AddLogging(configure => configure.AddConsole());

// Register Infrastructure and Application layers
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();

// Register App orchestrator
builder.Services.AddSingleton<App>();

using IHost host = builder.Build();

// Run Application
var app = host.Services.GetRequiredService<App>();
await app.RunAsync(CancellationToken.None);

/// <summary>
/// Main Application orchestrator for Console UX.
/// </summary>
public sealed class App(
    ILogger<App> logger, 
    IServiceProvider serviceProvider)
{
    private const string Banner = """
        **********************************************************
        *                UTM MARKET - SISTEMA POS                *
        *           Gestión de Productos e Inventarios           *
        **********************************************************
        """;

    public async Task RunAsync(CancellationToken ct)
    {
        Console.WriteLine(Banner);
        bool exit = false;

        while (!exit)
        {
            Console.WriteLine("""
            
            MENÚ PRINCIPAL:
            1. Gestión de Inventario (Productos)
            2. Gestión de Clientes (Loyalty)
            3. Consultar Historial de Ventas por Fecha
            4. Salir
            """);
            
            Console.Write("\nSeleccione una opción: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ShowInventoryMenuAsync(ct);
                    break;
                case "2":
                    await ShowCustomerMenuAsync(ct);
                    break;
                case "3":
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var fetchSalesUseCase = scope.ServiceProvider.GetRequiredService<IFetchSalesByFilterUseCase>();
                        await SaleUIHandler.ShowSalesByDateRangeAsync(fetchSalesUseCase, ct);
                    }
                    break;
                case "4":
                    exit = true;
                    Console.WriteLine("Cerrando sistema...");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Opción no válida. Intente de nuevo.");
                    Console.ResetColor();
                    break;
            }
        }
    }

    private async Task ShowInventoryMenuAsync(CancellationToken ct)
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine("""
            
            --- GESTIÓN DE INVENTARIO ---
            1. Consultar todos los productos
            2. Consultar producto por ID
            3. Registrar nuevo producto
            4. Consultar productos con bajo inventario
            5. Volver al menú principal
            """);

            Console.Write("\nSeleccione una opción: ");
            string? choice = Console.ReadLine();

            using var scope = serviceProvider.CreateScope();

            switch (choice)
            {
                case "1":
                    var listAllUseCase = scope.ServiceProvider.GetRequiredService<IGetAllProductsUseCase>();
                    await ProductUIHandler.ShowAllProductsAsync(listAllUseCase, ct);
                    break;
                case "2":
                    var getByIdUseCase = scope.ServiceProvider.GetRequiredService<IGetProductByIdUseCase>();
                    await ProductUIHandler.ShowProductByIdAsync(getByIdUseCase, ct);
                    break;
                case "3":
                    var createUseCase = scope.ServiceProvider.GetRequiredService<ICreateProductUseCase>();
                    await ProductUIHandler.RegisterProductAsync(createUseCase, ct);
                    break;
                case "4":
                    var lowStockUseCase = scope.ServiceProvider.GetRequiredService<ILowStockAlertUseCase>();
                    await ProductUIHandler.ShowLowStockAlertsAsync(lowStockUseCase, ct);
                    break;
                case "5":
                    back = true;
                    break;
                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }
    }

    private async Task ShowCustomerMenuAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        
        var getAll = scope.ServiceProvider.GetRequiredService<IGetAllCustomersUseCase>();
        var getByEmail = scope.ServiceProvider.GetRequiredService<IGetCustomerByEmailUseCase>();
        var register = scope.ServiceProvider.GetRequiredService<IRegisterCustomerUseCase>();

        await CustomerUIHandler.ShowCustomerSubMenuAsync(getAll, getByEmail, register, ct);
    }
}
