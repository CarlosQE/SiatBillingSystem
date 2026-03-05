namespace SiatBillingSystem.Domain.Entities;

/// <summary>
/// Configuración de la empresa emisora de facturas.
/// En la versión inicial (single-tenant), solo existe un registro en la BD.
/// La arquitectura está preparada para escalar a multi-NIT agregando TenantId.
/// </summary>
public class ConfiguracionEmpresa
{
    public int Id { get; set; }

    // ── Datos fiscales obligatorios ──

    /// <summary>NIT de la empresa registrado en el SIN.</summary>
    public string Nit { get; set; } = string.Empty;

    /// <summary>Razón social exacta como aparece en el padrón del SIN.</summary>
    public string RazonSocial { get; set; } = string.Empty;

    /// <summary>Código de la modalidad de facturación (catálogo SIN).</summary>
    public int CodigoModalidad { get; set; }

    /// <summary>Código de sucursal. 0 = Casa Matriz.</summary>
    public int CodigoSucursal { get; set; } = 0;

    /// <summary>Código de punto de venta. Null si no aplica.</summary>
    public int? CodigoPuntoVenta { get; set; }

    /// <summary>
    /// Actividad económica CAEB principal.
    /// Ejemplo: "620000" para actividades de programación informática.
    /// </summary>
    public string ActividadEconomica { get; set; } = string.Empty;

    /// <summary>
    /// Leyenda de la Ley 453 asignada a la actividad económica.
    /// Se imprime en todas las facturas. El SIN la asigna por catálogo.
    /// </summary>
    public string LeyendaLey453 { get; set; } = string.Empty;

    // ── Certificado digital ──

    /// <summary>
    /// Ruta al archivo .p12/.pfx del certificado digital ADSIB o DigiCert.
    /// SEGURIDAD: Esta ruta se almacena en texto plano pero la contraseña
    /// se guarda cifrada con DPAPI en PasswordCertificadoCifrado.
    /// </summary>
    public string RutaCertificado { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del certificado cifrada con Windows DPAPI.
    /// NUNCA almacenar la contraseña en texto plano.
    /// </summary>
    public string PasswordCertificadoCifrado { get; set; } = string.Empty;

    // ── CUFD (Código Único de Facturación Diaria) ──

    /// <summary>CUFD vigente. Válido por 24 horas desde FechaCufd.</summary>
    public string Cufd { get; set; } = string.Empty;

    /// <summary>Fecha y hora en que se obtuvo el CUFD actual.</summary>
    public DateTime? FechaCufd { get; set; }

    /// <summary>Fecha y hora de vencimiento del CUFD (normalmente 24h después).</summary>
    public DateTime? VencimientoCufd { get; set; }

    // ── Control del número de factura ──

    /// <summary>
    /// Último número de factura emitido.
    /// Se incrementa atómicamente en cada nueva factura para garantizar secuencia.
    /// </summary>
    public long UltimoNumeroFactura { get; set; } = 0;

    // ── Metadata ──

    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? FechaUltimaActualizacion { get; set; }

    /// <summary>
    /// Indica si el CUFD está próximo a vencer (menos de 10 minutos).
    /// Propiedad calculada — no se persiste en BD.
    /// </summary>
    public bool CufdProximoAVencer =>
        VencimientoCufd.HasValue &&
        (VencimientoCufd.Value - DateTime.Now).TotalMinutes < 10;

    /// <summary>
    /// Indica si el CUFD ya venció.
    /// Propiedad calculada — no se persiste en BD.
    /// </summary>
    public bool CufdVencido =>
        !VencimientoCufd.HasValue || DateTime.Now > VencimientoCufd.Value;
}
