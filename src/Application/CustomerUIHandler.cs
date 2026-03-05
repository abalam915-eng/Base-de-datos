using System.Text.RegularExpressions;
using UTM_Market.Core.Entities;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Manejador de la interfaz de usuario para la gestión de clientes.
/// </summary>
public static class CustomerUIHandler
{
    private const string Banner = """
        ----------------------------------------------------------
        |                GESTIÓN DE CLIENTES                     |
        ----------------------------------------------------------
        """;

    public static async Task ShowCustomerSubMenuAsync(
        IGetAllCustomersUseCase getAllUseCase,
        IGetCustomerByEmailUseCase getByEmailUseCase,
        IRegisterCustomerUseCase registerUseCase,
        CancellationToken ct)
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine(Banner);
            Console.WriteLine("""
            1. Listar todos los clientes
            2. Buscar cliente por Email
            3. Registrar nuevo cliente
            4. Volver al menú principal
            """);

            Console.Write("\nSeleccione una opción: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ListAllCustomersAsync(getAllUseCase, ct);
                    break;
                case "2":
                    await FindCustomerByEmailAsync(getByEmailUseCase, ct);
                    break;
                case "3":
                    await RegisterNewCustomerAsync(registerUseCase, ct);
                    break;
                case "4":
                    back = true;
                    break;
                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }
    }

    private static async Task ListAllCustomersAsync(IGetAllCustomersUseCase useCase, CancellationToken ct)
    {
        Console.WriteLine("\n--- LISTADO DE CLIENTES ---");
        int count = 0;
        await foreach (var customer in useCase.ExecuteAsync(ct))
        {
            Console.WriteLine($"{customer.ClienteID,-5} | {customer.NombreCompleto,-25} | {customer.Email,-30} | {(customer.EsActivo ? "Activo" : "Inactivo")}");
            count++;
        }

        if (count == 0) Console.WriteLine("No hay clientes registrados.");
        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private static async Task FindCustomerByEmailAsync(IGetCustomerByEmailUseCase useCase, CancellationToken ct)
    {
        Console.Write("\nIngrese el Email del cliente: ");
        string? email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
        {
            Console.WriteLine("Email inválido.");
            return;
        }

        var customer = await useCase.ExecuteAsync(email, ct);
        if (customer != null)
        {
            Console.WriteLine("\nCliente encontrado:");
            Console.WriteLine($"ID: {customer.ClienteID}");
            Console.WriteLine($"Nombre: {customer.NombreCompleto}");
            Console.WriteLine($"Email: {customer.Email}");
            Console.WriteLine($"Estado: {(customer.EsActivo ? "Activo" : "Inactivo")}");
            Console.WriteLine($"Fecha Registro: {customer.FechaRegistro:dd/MM/yyyy HH:mm}");
        }
        else
        {
            Console.WriteLine("Cliente no encontrado.");
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private static async Task RegisterNewCustomerAsync(IRegisterCustomerUseCase useCase, CancellationToken ct)
    {
        Console.WriteLine("\n--- REGISTRO DE NUEVO CLIENTE ---");
        
        Console.Write("Nombre Completo: ");
        string? name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("El nombre es obligatorio.");
            return;
        }

        Console.Write("Email: ");
        string? email = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
        {
            Console.WriteLine("El email es inválido.");
            return;
        }

        try
        {
            var customer = new Customer(0, name, DateTime.Now) { Email = email };
            var registered = await useCase.ExecuteAsync(customer, ct);
            Console.WriteLine($"\nCliente registrado exitosamente con ID: {registered.ClienteID}");
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
        }

        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
