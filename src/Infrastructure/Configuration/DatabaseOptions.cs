using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace UTM_Market.Infrastructure.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required, NotNull]
    public string DefaultConnection
    {
        get => field ?? string.Empty;
        set => field = string.IsNullOrWhiteSpace(value) 
            ? throw new ArgumentException("La cadena de conexión no puede ser nula o estar vacía.", nameof(value)) 
            : value;
    }
}
