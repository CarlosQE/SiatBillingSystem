using SiatBillingSystem.Domain.Constants;

namespace SiatBillingSystem.Domain.Entities;

/// <summary>
/// Entidad principal que representa una Factura del Sector Servicios según estándares SIAT.
///
/// REGLA DE ORO (Clean Architecture): Esta entidad no tiene dependencias externas.
/// Solo tipos primitivos de C# y referencias a otras entidades/constantes del mismo proyecto Domain.
/// </summary>
public class ServiceInvoice
{
    // ─────────────────────────────────────────────────────────────────────────────
    // IDENTIFICACIÓN DEL EMISOR Y PUNTO DE EMISIÓN
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>NIT del emisor (empresa que factura). String para evitar pérdida de ceros iniciales.</summary>
    public string NitEmisor { get; set; } = string.Empty;

    /// <summary>Número secuencial de la factura. Long para soportar volúmenes altos.</summary>
    public long NumeroFactura { get; set; }

    /// <summary>Código Único de Factura (CUF). Calculado por SiatAlgorithms.GenerarCUF().</summary>
    public string Cuf { get; set; } = string.Empty;

    /// <summary>Código Único de Facturación Diaria. Válido por 24 horas. Obtenido del SIN.</summary>
    public string Cufd { get; set; } = string.Empty;

    /// <summary>Código de sucursal. 0 = Casa Matriz.</summary>
    public int CodigoSucursal { get; set; } = 0;

    /// <summary>Código de punto de venta. Null si no aplica (casa matriz sin POS diferenciado).</summary>
    public int? CodigoPuntoVenta { get; set; }

    // ─────────────────────────────────────────────────────────────────────────────
    // MODALIDAD Y TIPO DE EMISIÓN (campos requeridos para GenerarCUF)
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Modalidad de facturación según catálogo SIN.
    /// Ver SiatConstants.Modalidad* para valores válidos.
    /// Ejemplo: 1 = Electrónica en Línea, 2 = Computarizada en Línea.
    /// </summary>
    public int CodigoModalidad { get; set; } = SiatConstants.ModalidadComputarizadaEnLinea;

    /// <summary>
    /// Tipo de emisión según catálogo SIN.
    /// 1 = Online (normal), 2 = Offline (contingencia por corte de internet).
    /// </summary>
    public int TipoEmision { get; set; } = SiatConstants.TipoEmisionOnline;

    /// <summary>
    /// Tipo de factura según catálogo SIN.
    /// 1 = Con derecho a crédito fiscal (empresas con NIT).
    /// 2 = Sin derecho a crédito fiscal (consumidor final con CI).
    /// </summary>
    public int TipoFactura { get; set; } = SiatConstants.TipoFacturaConDerechoCreditoFiscal;

    /// <summary>
    /// Tipo de documento de sector según catálogo SIN.
    /// Ejemplo: 1 = Compra/Venta general, 12 = Servicios Turísticos.
    /// Para servicios generales, consultar catálogo vigente del SIN.
    /// </summary>
    public int TipoDocumentoSector { get; set; } = SiatConstants.SectorCompraVenta;

    // ─────────────────────────────────────────────────────────────────────────────
    // DATOS TEMPORALES
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fecha y hora de emisión de la factura.
    /// CRÍTICO: Incluir milisegundos para el CUF. Usar DateTime.Now (no UtcNow) — el SIN trabaja en hora local Bolivia (UTC-4).
    /// </summary>
    public DateTime FechaEmision { get; set; }

    // ─────────────────────────────────────────────────────────────────────────────
    // DATOS DEL CLIENTE
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Nombre o razón social del cliente receptor de la factura.</summary>
    public string NombreRazonSocial { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de documento de identidad del cliente según catálogo SIN.
    /// 1 = Cédula de Identidad, 5 = NIT.
    /// </summary>
    public int CodigoTipoDocumentoIdentidad { get; set; } = SiatConstants.TipoDocumentoCedulaIdentidad;

    /// <summary>Número del documento de identidad del cliente.</summary>
    public string NumeroDocumento { get; set; } = string.Empty;

    /// <summary>Complemento del CI (ej: "1A"). Null si no aplica.</summary>
    public string? Complemento { get; set; }

    // ─────────────────────────────────────────────────────────────────────────────
    // IMPORTES Y MÉTODO DE PAGO
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Monto total de la factura en Bolivianos.
    /// CRÍTICO: Siempre decimal, nunca float/double para evitar errores de redondeo fiscal.
    /// </summary>
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Monto base sujeto a IVA (13% según ley boliviana).
    /// Para servicios generales: MontoTotalSujetoIva = MontoTotal.
    /// </summary>
    public decimal MontoTotalSujetoIva { get; set; }

    /// <summary>
    /// Método de pago según catálogo SIN.
    /// Ejemplo: 1 = Efectivo, 2 = Tarjeta, 7 = Transferencia bancaria.
    /// </summary>
    public int CodigoMetodoPago { get; set; }

    // ─────────────────────────────────────────────────────────────────────────────
    // LEYENDA LEGAL (Ley 453 — Derechos del Consumidor)
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Leyenda obligatoria según Ley 453 de Derechos de Usuarios y Consumidores.
    /// Se asigna dinámicamente según la actividad económica del servicio prestado.
    /// El SIN provee el catálogo de leyendas por actividad económica.
    /// </summary>
    public string Leyenda { get; set; } = string.Empty;

    // ─────────────────────────────────────────────────────────────────────────────
    // DETALLE DE LA FACTURA
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lista de ítems de servicio facturados.
    /// Una factura de servicios puede tener múltiples líneas (ej: honorarios + gastos de gestión).
    /// </summary>
    public List<ServiceInvoiceDetail> Details { get; set; } = new();

    // ─────────────────────────────────────────────────────────────────────────────
    // CONSTRUCTOR
    // ─────────────────────────────────────────────────────────────────────────────

    public ServiceInvoice()
    {
        // Hora local Bolivia (UTC-4). NUNCA usar DateTime.UtcNow para facturas SIAT.
        FechaEmision = DateTime.Now;
    }
}

/// <summary>
/// Representa una línea de detalle dentro de la factura de servicios.
///
/// Diferencia clave vs. sector comercial: no hay manejo de stock ni kardex.
/// El campo Descripcion puede contener texto extenso (ej: "Honorarios profesionales
/// por auditoría financiera del período enero-diciembre 2024 según contrato N° 123").
/// </summary>
public class ServiceInvoiceDetail
{
    /// <summary>
    /// Código de actividad económica del SIN (CAEB).
    /// Ejemplo: "620000" = Actividades de programación informática.
    /// </summary>
    public string ActividadEconomica { get; set; } = string.Empty;

    /// <summary>Código de producto según catálogo SIN. Para servicios: usar código 99900 (genérico).</summary>
    public int CodigoProductoSin { get; set; }

    /// <summary>Código interno del producto/servicio en el sistema del cliente.</summary>
    public string CodigoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del servicio prestado. Admite texto extenso.
    /// Es el campo diferenciador clave del sector servicios vs. comercio.
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Cantidad del servicio (ej: 1.5 horas, 3 meses de suscripción).</summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Unidad de medida según catálogo SIN.
    /// Para servicios: código 58 = "Servicio" (valor por defecto según normativa).
    /// </summary>
    public int UnidadMedida { get; set; } = 58;

    /// <summary>Precio unitario del servicio en Bolivianos. Siempre decimal.</summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Subtotal de esta línea: Cantidad * PrecioUnitario.
    /// Se calcula y almacena explícitamente para evitar recálculos y facilitar auditoría.
    /// </summary>
    public decimal SubTotal { get; set; }
}
