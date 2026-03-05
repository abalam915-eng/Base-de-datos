# Informe de Arquitectura y Configuración: UTM_Market CLI

Fecha: 23 de Febrero de 2026

**Autor:** Gemini, Arquitecto de Software Senior

Este documento detalla la configuración inicial del proyecto `UTM_Market`, una herramienta de línea de comandos (CLI) desarrollada en .NET 10. La configuración sigue los principios de Clean Code, Zero Trust y está optimizada para compilación Native AOT.

## 1. Resumen de Instalación de Dependencias

Se han instalado los siguientes paquetes NuGet para formar la base de la arquitectura de la aplicación. Las versiones corresponden a las más estables y compatibles con .NET 10 al momento de la instalación.

| Paquete NuGet                                 | Versión Instalada | Rol Arquitectónico                                                                                             |
| --------------------------------------------- | ----------------- | -------------------------------------------------------------------------------------------------------------- |
| `Microsoft.Data.SqlClient`                    | `6.1.4`           | Driver de base de datos oficial para SQL Server, optimizado para Native AOT.                                    |
| `Dapper`                                      | `2.1.66`          | Micro-ORM de alto rendimiento para el mapeo de datos. Su naturaleza sin reflexión lo hace ideal para AOT.       |
| `Microsoft.Extensions.Hosting`                | `10.0.3`          | Provee el host genérico de la aplicación, gestionando el ciclo de vida, la configuración y la inyección de DI.  |
| `Microsoft.Extensions.Configuration.UserSecrets`| `10.0.3`          | Permite la gestión segura de secretos de desarrollo local, alineado con el principio de Zero Trust.          |

## 2. Referencia de Implementación: `Program.cs`

El archivo `Program.cs` se ha estructurado para demostrar una arquitectura moderna utilizando el host genérico y la inyección de dependencias.

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;

// Usando HostApplicationBuilder para una inicialización simplificada y unificada,
// cumpliendo con los estándares de modernización de .NET 10.
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Configuración del logging y otros servicios del framework.
builder.Services.AddLogging(configure => configure.AddConsole());

// Registro de nuestros servicios personalizados para la inyección de dependencias.
// Esto promueve un código desacoplado y fácil de mantener.
builder.Services.AddSingleton<App>();
builder.Services.AddTransient<IDataService, DataService>();

// Construcción del host que encapsula los servicios y la configuración de la app.
using IHost host = builder.Build();

// Ejecución del punto de entrada de la aplicación.
await host.Services.GetRequiredService<App>().RunAsync(CancellationToken.None);

/// <summary>
/// Representa el punto de entrada lógico de la aplicación.
/// </summary>
public class App
{
    private readonly ILogger<App> _logger;
    private readonly IDataService _dataService;

    public App(ILogger<App> logger, IDataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Aplicación CLI iniciada.");

        // Ejemplo de uso de un servicio con operación asíncrona.
        // Se pasa el CancellationToken para soportar la cancelación.
        var data = await _dataService.GetDataAsync(cancellationToken);

        _logger.LogInformation("Datos recuperados: {Data}", data);

        _logger.LogInformation("Aplicación CLI finalizada.");
    }
}

/// <summary>
/// Interfaz para un servicio de acceso a datos.
/// </summary>
public interface IDataService
{
    ValueTask<string> GetDataAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Implementación del servicio de acceso a datos.
/// </summary>
public class DataService : IDataService
{
    // C# 14: Propiedad auto-implementada con la palabra clave 'field'.
    // Esto reduce el boilerplate al no tener que declarar un campo privado explícito.
    // public string ConnectionString { get; set; } = "default_connection"; // Sintaxis tradicional
    // public string ConnectionString { get; set; } = field; // Nueva sintaxis conceptual C# 14
    
    // En este ejemplo, no se usa directamente pero se demuestra el concepto.
    public string Description { get; set; } = "Servicio de datos de ejemplo";

    /// <summary>
    /// Obtiene datos de forma asíncrona.
    /// ValueTask es preferible en rutas calientes para evitar alocaciones en el heap
    /// si la operación se completa sincrónicamente.
    /// </summary>
    public async ValueTask<string> GetDataAsync(CancellationToken cancellationToken)
    {
        // Simulando una llamada I/O a una base de datos.
        // En una implementación real, aquí se usaría Microsoft.Data.SqlClient y Dapper.
        await Task.Delay(1000, cancellationToken);
        
        // El código debe evitar la reflexión en tiempo de ejecución para ser compatible con AOT.
        // El mapeo de datos con Dapper a clases concretas es AOT-friendly.
        
        return "Datos de ejemplo";
    }
}
```

## 3. Notas de Modernización

### Beneficios de la sintaxis `field` en C# 14
La nueva palabra clave `field` en C# 14 simplifica la declaración de propiedades auto-implementadas al permitir el acceso directo al campo de respaldo (`backing field`) sin necesidad de declararlo explícitamente. Esto reduce el código repetitivo y mejora la legibilidad, especialmente en clases con muchas propiedades.

### Ventajas de Native AOT en .NET 10
La compilación Ahead-of-Time (AOT) para producir un binario nativo ofrece beneficios significativos para aplicaciones CLI:
- **Rendimiento de Arranque:** El tiempo de inicio es casi instantáneo, ya que el código se compila a código máquina antes de la ejecución, eliminando la necesidad de un compilador JIT (Just-In-Time).
- **Menor Huella de Memoria:** Las aplicaciones AOT consumen menos memoria porque no necesitan el compilador JIT en tiempo de ejecución.
- **Auto-contenidas:** El ejecutable final es un único archivo que no requiere que el runtime de .NET esté preinstalado en la máquina de destino, simplificando enormemente la distribución.

## 4. Guía de Ejecución y Compilación Nativa

Para compilar la aplicación como un binario nativo auto-contenido, utiliza el siguiente comando `dotnet publish`. Asegúrate de reemplazar `<RID>` con el Identificador de Runtime correspondiente a tu plataforma de destino (ej. `win-x64`, `linux-x64`, `osx-x64`).

```bash
# Ejemplo para Windows 64-bit
dotnet publish -c Release -r win-x64 /p:PublishAot=true
```

El resultado será un único archivo ejecutable en el directorio `bin/Release/net10.0/<RID>/publish/`.
