using System.Text.RegularExpressions;

namespace UTM_Market.Core.Entities;

/// <summary>
/// Representa el núcleo de negocio de un Cliente para el módulo de lealtad.
/// Implementado como un POCO puro compatible con Native AOT y C# 14.
/// </summary>
/// <param name="ClienteID">Identificador único del cliente.</param>
/// <param name="NombreCompleto">Nombre y apellidos del cliente.</param>
/// <param name="FechaRegistro">Fecha de alta en el sistema.</param>
public class Customer(int ClienteID, string NombreCompleto, DateTime FechaRegistro)
{
    /// <summary>
    /// Identificador único (Primary Key).
    /// </summary>
    public int ClienteID { get; } = ClienteID;

    /// <summary>
    /// Nombre completo del cliente.
    /// </summary>
    public string NombreCompleto { get; set; } = NombreCompleto;

    /// <summary>
    /// Fecha de registro en el sistema.
    /// </summary>
    public DateTime FechaRegistro { get; } = FechaRegistro;

    /// <summary>
    /// Estado de actividad del cliente para borrado lógico.
    /// </summary>
    public bool EsActivo { get; set; } = true;

    /// <summary>
    /// Correo electrónico validado con lógica de negocio encapsulada.
    /// Utiliza la palabra clave 'field' de C# 14 para reducir boilerplate.
    /// </summary>
    public string Email
    {
        get => field;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@') || !value.Contains('.'))
            {
                throw new ArgumentException("El formato del correo electrónico es inválido.");
            }
            field = value;
        }
    }

    /// <summary>
    /// Propiedad calculada que determina si el cliente es nuevo (menos de 30 días).
    /// </summary>
    public bool EsNuevo => (DateTime.Now - FechaRegistro).TotalDays <= 30;

    /// <summary>
    /// Representación textual del objeto para depuración.
    /// </summary>
    public override string ToString() => $"[ID: {ClienteID}] {NombreCompleto} ({Email})";
}
