namespace UTM_Market.Infrastructure.Models.Data;

/// <summary>
/// Representación física de la tabla 'Clientes' en SQL Server 2022.
/// Diseñada para compatibilidad total con Native AOT mediante mapeo manual (sin reflexión).
/// </summary>
/// <param name="clienteID">Identificador correlativo de la base de datos.</param>
/// <param name="nombreCompleto">Nombre y apellidos (NVARCHAR(150)).</param>
/// <param name="email">Correo electrónico validado (VARCHAR(100)).</param>
/// <param name="esActivo">Estado lógico del registro (BIT).</param>
/// <param name="fechaRegistro">Fecha de inserción en el sistema (DATETIME).</param>
public partial class CustomerEntity(int clienteID, string nombreCompleto, string email, bool esActivo, DateTime fechaRegistro)
{
    /// <summary>
    /// Identificador primario de la tabla 'Clientes'.
    /// </summary>
    public int ClienteID { get; set; } = clienteID;

    /// <summary>
    /// Mapeo 1:1 con la columna [NombreCompleto].
    /// Implementa validación inmediata usando la palabra clave 'field' de C# 14.
    /// </summary>
    public string NombreCompleto
    {
        get => field;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            {
                throw new ArgumentException("El campo NombreCompleto es obligatorio y no debe exceder los 150 caracteres.");
            }
            field = value;
        }
    } = nombreCompleto;

    /// <summary>
    /// Mapeo 1:1 con la columna [Email].
    /// Refuerza la restricción CHECK de SQL Server en el lado de la aplicación.
    /// </summary>
    public string Email
    {
        get => field;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 100 || !value.Contains('@') || !value.Contains('.'))
            {
                throw new ArgumentException("El formato del Email es inválido o excede los 100 caracteres.");
            }
            field = value;
        }
    } = email;

    /// <summary>
    /// Estado de actividad. Refleja la columna [EsActivo].
    /// </summary>
    public bool EsActivo { get; set; } = esActivo;

    /// <summary>
    /// Timestamp de auditoría. Refleja la columna [FechaRegistro].
    /// </summary>
    public DateTime FechaRegistro { get; set; } = fechaRegistro;

    /// <summary>
    /// Constructor vacío requerido para mappers manuales y flexibilidad de inicialización.
    /// </summary>
    public CustomerEntity() : this(0, string.Empty, "placeholder@temp.com", true, DateTime.Now) { }
}
