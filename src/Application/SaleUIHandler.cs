using System.Globalization;
using UTM_Market.Core.Entities;
using UTM_Market.Core.Filters;
using UTM_Market.Core.UseCases;

namespace UTM_Market.Application;

/// <summary>
/// Helper class to handle Console UI interactions for Sale management.
/// Follows the pattern established in the project for Product and Customer UI.
/// </summary>
public sealed class SaleUIHandler
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Displays a sub-menu to search for sales by date range.
    /// Includes resilient input capture and tabular formatting.
    /// </summary>
    public static async Task ShowSalesByDateRangeAsync(IFetchSalesByFilterUseCase useCase, CancellationToken ct)
    {
        Console.WriteLine("""

        --- CONSULTA DE HISTORIAL DE VENTAS ---
        """);

        DateTime? startDate = ReadDate("Fecha de Inicio (formato: yyyy-MM-dd): ");
        if (startDate == null) return;

        DateTime? endDate = null;
        bool validRange = false;
        int attempts = 0;

        while (!validRange && attempts < 3)
        {
            endDate = ReadDate("Fecha de Fin (formato: yyyy-MM-dd): ");
            if (endDate == null) return;

            if (endDate >= startDate)
            {
                validRange = true;
            }
            else
            {
                attempts++;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Error: La Fecha de Fin no puede ser anterior a la Fecha de Inicio (Intento {attempts}/3).");
                Console.ResetColor();
            }
        }

        if (!validRange)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Máximo de intentos alcanzado. Operación cancelada.");
            Console.ResetColor();
            return;
        }

        var filter = new SaleFilter 
        { 
            StartDate = startDate, 
            EndDate = endDate?.AddDays(1).AddSeconds(-1) // Include full end day
        };

        Console.WriteLine($"""

        --- RESULTADOS: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy} ---
        """);

        // Tabular Header
        Console.WriteLine("{0,-15} | {1,20} | {2,15}", "FOLIO", "FECHA", "MONTO TOTAL");
        Console.WriteLine(new string('-', 55));

        int count = 0;
        decimal grandTotal = 0;

        await foreach (var sale in useCase.ExecuteAsync(filter, ct))
        {
            Console.WriteLine("{0,-15} | {1,20:dd/MM/yyyy HH:mm:ss} | {2,15:C}", 
                sale.Folio, sale.SaleDate, sale.TotalSale);
            
            count++;
            grandTotal += sale.TotalSale;
        }

        if (count == 0)
        {
            Console.WriteLine("\nNo se encontraron ventas en el rango de fechas especificado.");
        }
        else
        {
            Console.WriteLine(new string('-', 55));
            Console.WriteLine("{0,-15} | {1,20} | {2,15:C}", "TOTALES", $"{count} Ventas", grandTotal);
        }
        
        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private static DateTime? ReadDate(string prompt)
    {
        int attempts = 0;
        while (attempts < 3)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (DateTime.TryParseExact(input, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            attempts++;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Formato inválido. Use {DateFormat} (Ej: 2025-03-04). Intento {attempts}/3.");
            Console.ResetColor();
        }

        return null;
    }
}
