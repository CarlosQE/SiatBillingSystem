namespace SiatBillingSystem.Domain.Entities;

/// <summary>
/// Representa un cliente frecuente guardado en la base de datos local.
/// Permite autocompletar datos en la grilla POS sin tener que reescribir
/// NIT/CI y nombre cada vez — clave para el caso de uso de fisioterapia
/// donde los mismos pacientes reciben servicios mes a mes.
/// </summary>
public class ClienteFrecuente
{
    public int Id { get; set; }

    /// <summary>
    /// Tipo de documento: 1=CI, 5=NIT (catálogo SIN).
    /// Determina si es consumidor final (CI) o empresa con crédito fiscal (NIT).
    /// </summary>
    public int CodigoTipoDocumento { get; set; }

    /// <summary>Número del documento de identidad o NIT.</summary>
    public string NumeroDocumento { get; set; } = string.Empty;

    /// <summary>Complemento del CI (ej: "1A"). Null si no aplica.</summary>
    public string? Complemento { get; set; }

    /// <summary>Nombre completo o razón social.</summary>
    public string NombreRazonSocial { get; set; } = string.Empty;

    /// <summary>Teléfono opcional para contacto. No requerido por el SIN.</summary>
    public string? Telefono { get; set; }

    /// <summary>Email opcional. No requerido por el SIN.</summary>
    public string? Email { get; set; }

    /// <summary>Fecha de registro del cliente en el sistema.</summary>
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    /// <summary>Última vez que se le emitió una factura. Para ordenar por frecuencia.</summary>
    public DateTime? UltimaFactura { get; set; }

    /// <summary>Total de facturas emitidas a este cliente. Para estadísticas rápidas.</summary>
    public int TotalFacturas { get; set; } = 0;

    /// <summary>Navegación hacia las facturas emitidas a este cliente.</summary>
    public List<ServiceInvoice> Facturas { get; set; } = new();
}
