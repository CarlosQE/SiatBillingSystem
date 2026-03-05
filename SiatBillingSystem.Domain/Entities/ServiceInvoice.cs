using SiatBillingSystem.Domain.Constants;
using SiatBillingSystem.Domain.Enums;

namespace SiatBillingSystem.Domain.Entities;

/// <summary>
/// Entidad principal — Factura del Sector Servicios según estándares SIAT.
/// Ahora incluye propiedades de persistencia (Id, FK) y estado del ciclo de vida.
/// </summary>
public class ServiceInvoice
{
    // ─────────────────────────────────────────────────────────────────────────────
    // PERSISTENCIA
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Clave primaria local (SQLite autoincrement).</summary>
    public int Id { get; set; }

    /// <summary>FK opcional al cliente frecuente. Null si se ingresó manualmente.</summary>
    public int? ClienteFrecuenteId { get; set; }

    /// <summary>Navegación al cliente frecuente.</summary>
    public ClienteFrecuente? ClienteFrecuente { get; set; }

    // ─────────────────────────────────────────────────────────────────────────────
    // ESTADO DEL CICLO DE VIDA (Offline-First)
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Estado actual respecto al SIN.</summary>
    public EstadoEnvioSin EstadoEnvio { get; set; } = EstadoEnvioSin.PendienteEnvio;

    /// <summary>Código de autorización devuelto por el SIN al aceptar la factura.</summary>
    public string? CodigoAutorizacion { get; set; }

    /// <summary>Motivo de rechazo del SIN. Null si fue aceptada.</summary>
    public string? MotivoRechazo { get; set; }

    /// <summary>Fecha en que el SIN procesó la factura (aceptó o rechazó).</summary>
    public DateTime? FechaRespuestaSin { get; set; }

    /// <summary>XML firmado listo para enviar al SIN. Se guarda para reenvíos.</summary>
    public string? XmlFirmado { get; set; }

    /// <summary>Número de intentos de envío al SIN. Para control de reintentos.</summary>
    public int IntentosEnvio { get; set; } = 0;

    // ─────────────────────────────────────────────────────────────────────────────
    // IDENTIFICACIÓN DEL EMISOR
    // ─────────────────────────────────────────────────────────────────────────────

    public string NitEmisor { get; set; } = string.Empty;
    public long NumeroFactura { get; set; }
    public string Cuf { get; set; } = string.Empty;
    public string Cufd { get; set; } = string.Empty;
    public int CodigoSucursal { get; set; } = 0;
    public int? CodigoPuntoVenta { get; set; }

    // ─────────────────────────────────────────────────────────────────────────────
    // MODALIDAD Y TIPO
    // ─────────────────────────────────────────────────────────────────────────────

    public int CodigoModalidad { get; set; } = SiatConstants.ModalidadComputarizadaEnLinea;
    public int TipoEmision { get; set; } = SiatConstants.TipoEmisionOnline;
    public int TipoFactura { get; set; } = SiatConstants.TipoFacturaConDerechoCreditoFiscal;
    public int TipoDocumentoSector { get; set; } = SiatConstants.SectorCompraVenta;

    // ─────────────────────────────────────────────────────────────────────────────
    // FECHA
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Hora local Bolivia (UTC-4). NUNCA usar DateTime.UtcNow para facturas SIAT.
    /// </summary>
    public DateTime FechaEmision { get; set; }

    // ─────────────────────────────────────────────────────────────────────────────
    // DATOS DEL CLIENTE
    // ─────────────────────────────────────────────────────────────────────────────

    public string NombreRazonSocial { get; set; } = string.Empty;
    public int CodigoTipoDocumentoIdentidad { get; set; } = SiatConstants.TipoDocumentoCedulaIdentidad;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Complemento { get; set; }

    // ─────────────────────────────────────────────────────────────────────────────
    // IMPORTES
    // ─────────────────────────────────────────────────────────────────────────────

    public decimal MontoTotal { get; set; }
    public decimal MontoTotalSujetoIva { get; set; }
    public int CodigoMetodoPago { get; set; }
    public string Leyenda { get; set; } = string.Empty;

    // ─────────────────────────────────────────────────────────────────────────────
    // DETALLE
    // ─────────────────────────────────────────────────────────────────────────────

    public List<ServiceInvoiceDetail> Details { get; set; } = new();

    // ─────────────────────────────────────────────────────────────────────────────
    // CONSTRUCTOR
    // ─────────────────────────────────────────────────────────────────────────────

    public ServiceInvoice()
    {
        FechaEmision = DateTime.Now;
    }
}

public class ServiceInvoiceDetail
{
    public int Id { get; set; }

    /// <summary>FK a la factura padre.</summary>
    public int ServiceInvoiceId { get; set; }
    public ServiceInvoice? ServiceInvoice { get; set; }

    public string ActividadEconomica { get; set; } = string.Empty;
    public int CodigoProductoSin { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public int UnidadMedida { get; set; } = 58; // 58 = Servicio (catálogo SIN)
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }
}
