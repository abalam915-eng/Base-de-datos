using UTM_Market.Core.Entities;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Helper class to handle Console UI interactions for Product management.
/// </summary>
public sealed class ProductUIHandler
{
    // C# 14 UTF-8 string literals (u8)
    private static ReadOnlySpan<byte> SuccessTag => "[EXITO]"u8;
    private static ReadOnlySpan<byte> ErrorTag => "[ERROR]"u8;

    public static async Task ShowAllProductsAsync(IGetAllProductsUseCase useCase, CancellationToken ct)
    {
        Console.WriteLine("""

        --- LISTADO DE PRODUCTOS ---
        """);
        
        // Use composite formatting for the header
        Console.WriteLine("{0,-5} | {1,-10} | {2,-25} | {3,-10} | {4,-5}", "ID", "SKU", "NOMBRE", "PRECIO", "STOCK");
        Console.WriteLine(new string('-', 65));

        int count = 0;
        await foreach (var p in useCase.ExecuteAsync(ct))
        {
            Console.WriteLine("{0,-5} | {1,-10} | {2,-25} | {3,10:C} | {4,5}", 
                p.ProductID, p.SKU, p.Name, p.Price, p.Stock);
            count++;
        }

        if (count == 0) Console.WriteLine("No se encontraron productos.");
        Console.WriteLine($"""

        Total: {count} productos.
        """);
    }

    public static async Task ShowProductByIdAsync(IGetProductByIdUseCase useCase, CancellationToken ct)
    {
        int id = ReadInt("Ingrese el ID del producto: ");
        var p = await useCase.ExecuteAsync(id, ct);

        if (p == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Producto con ID {id} no encontrado.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("""

        --- DETALLE DEL PRODUCTO ---
        """);
        Console.WriteLine($"ID:     {p.ProductID}");
        Console.WriteLine($"SKU:    {p.SKU}");
        Console.WriteLine($"Nombre: {p.Name}");
        Console.WriteLine($"Marca:  {p.Brand}");
        Console.WriteLine($"Precio: {p.Price:C}");
        Console.WriteLine($"Stock:  {p.Stock}");
    }

    public static async Task RegisterProductAsync(ICreateProductUseCase useCase, CancellationToken ct)
    {
        Console.WriteLine("""

        --- REGISTRAR NUEVO PRODUCTO ---
        """);
        
        string name = ReadString("Nombre: ");
        string sku = ReadString("SKU: ");
        string brand = ReadString("Marca: ");
        decimal price = ReadDecimal("Precio: ");
        int stock = ReadInt("Stock Inicial: ");

        try 
        {
            // ProductID is 0 for new products (DB generated)
            var product = new Product(0, name, sku, brand)
            {
                Price = price,
                Stock = stock
            };

            await useCase.ExecuteAsync(product, ct);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n");
            Console.Out.Write(System.Text.Encoding.UTF8.GetString(SuccessTag));
            Console.WriteLine(" Producto registrado correctamente.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n");
            Console.Out.Write(System.Text.Encoding.UTF8.GetString(ErrorTag));
            Console.WriteLine($" {ex.Message}");
            Console.ResetColor();
        }
    }

    public static async Task ShowLowStockAlertsAsync(ILowStockAlertUseCase useCase, CancellationToken ct)
    {
        int threshold;
        while (true)
        {
            threshold = ReadInt("Ingrese el Umbral de Stock: ");
            if (threshold > 0) break;
            Console.WriteLine("Error: El umbral debe ser un número entero positivo.");
        }

        Console.WriteLine($"""

        --- PRODUCTOS CON BAJO INVENTARIO (Stock <= {threshold}) ---
        """);

        Console.WriteLine("{0,-5} | {1,-25} | {2,-10} | {3,-5}", "ID", "NOMBRE", "PRECIO", "STOCK");
        Console.WriteLine(new string('-', 55));

        int count = 0;
        try
        {
            await foreach (var p in useCase.ExecuteAsync(threshold, ct))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0,-5} | {1,-25} | {2,10:C} | {3,5}",
                    p.ProductID, p.Name, p.Price, p.Stock);
                Console.ResetColor();
                count++;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error inesperado: {ex.Message}");
            Console.ResetColor();
        }

        if (count == 0) Console.WriteLine("No se encontraron productos con stock crítico.");
        Console.WriteLine($"""

        Total: {count} alertas encontradas.
        """);
    }

    #region Input Helpers

    private static string ReadString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) return input.Trim();
            Console.WriteLine("El valor no puede estar vacío.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int result)) return result;
            Console.WriteLine("Entrada inválida. Por favor, ingrese un número entero.");
        }
    }

    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), out decimal result)) return result;
            Console.WriteLine("Entrada inválida. Por favor, ingrese un valor decimal.");
        }
    }

    #endregion
}
