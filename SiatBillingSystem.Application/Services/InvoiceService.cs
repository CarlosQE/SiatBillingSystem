using System.Text;
using System.Xml;
using SiatBillingSystem.Application.Common;
using SiatBillingSystem.Application.Interfaces;
using SiatBillingSystem.Domain.Entities;
using SiatBillingSystem.Infrastructure.Interfaces;

namespace SiatBillingSystem.Application.Services;

/// <summary>
/// Servicio principal de facturación. Orquesta el flujo completo:
///   1. Calcular CUF (SiatAlgorithms)
///   2. Serializar factura a XML (formato solicitudServicioRecepcionFactura)
///   3. Firmar el XML (ISignatureService → SignatureService)
///
/// Este servicio vive en Application porque conoce las reglas de negocio del SIAT,
/// pero delega la criptografía a Infrastructure a través de la interfaz ISignatureService.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly ISignatureService _signatureService;

    public InvoiceService(ISignatureService signatureService)
    {
        _signatureService = signatureService;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // FLUJO PRINCIPAL
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Prepara la factura completa: CUF + XML + Firma.
    /// Retorna el XML firmado listo para empaquetar en el Envelope SOAP del SIN.
    /// </summary>
    public async Task<InvoiceResult> PrepararFacturaAsync(
        ServiceInvoice factura,
        string rutaCertificado,
        string passwordCertificado)
    {
        try
        {
            // ── Paso 1: Calcular y asignar el CUF ──
            factura.Cuf = SiatAlgorithms.GenerarCUF(factura);

            // ── Paso 2: Construir el XML según esquema SIAT solicitudServicioRecepcionFactura ──
            var xmlDoc = ConstruirXml(factura);

            // ── Paso 3: Cargar certificado y firmar el XML ──
            var certificado = _signatureService.CargarCertificado(rutaCertificado, passwordCertificado);
            _signatureService.FirmarXml(xmlDoc, certificado);

            // ── Paso 4: Serializar el XML firmado a string UTF-8 ──
            var xmlFirmado = SerializarXml(xmlDoc);

            // Task.FromResult porque la lógica es síncrona pero la interfaz es async
            // (preparado para cuando agreguemos validación XSD asíncrona en Sprint 1)
            return await Task.FromResult(InvoiceResult.Ok(factura.Cuf, xmlFirmado));
        }
        catch (Exception ex)
        {
            return await Task.FromResult(
                InvoiceResult.Fallo($"Error preparando factura N°{factura.NumeroFactura}: {ex.Message}"));
        }
    }

    /// <summary>
    /// Calcula el CUF sin generar el XML. Para pre-visualización en la UI.
    /// </summary>
    public string CalcularCuf(ServiceInvoice factura) =>
        SiatAlgorithms.GenerarCUF(factura);

    // ─────────────────────────────────────────────────────────────────────────────
    // CONSTRUCCIÓN DEL XML
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Construye el XML de la factura según el esquema oficial del SIAT:
    /// solicitudServicioRecepcionFactura → facturaServicioV3
    ///
    /// REGLAS CRÍTICAS del SIN (motivo de rechazo inmediato si se violan):
    ///   - Nodos vacíos prohibidos: si un campo es null, el nodo NO debe existir
    ///   - Fechas en formato ISO 8601: yyyy-MM-ddTHH:mm:ss.fff
    ///   - Decimales con punto (.) como separador, nunca coma
    ///   - Namespace correcto según versión del XSD
    /// </summary>
    private static XmlDocument ConstruirXml(ServiceInvoice factura)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));

        // Namespace oficial del esquema SIAT para sector servicios
        const string ns = "https://siat.impuestos.gob.bo/SIAT";

        var raiz = xmlDoc.CreateElement("solicitudServicioRecepcionFactura", ns);
        xmlDoc.AppendChild(raiz);

        // ── Cabecera ──
        AgregarNodo(xmlDoc, raiz, ns, "nitEmisor", factura.NitEmisor);
        AgregarNodo(xmlDoc, raiz, ns, "modalidad", factura.CodigoModalidad.ToString());
        AgregarNodo(xmlDoc, raiz, ns, "tipoEmisionFactura", factura.TipoEmision.ToString());
        AgregarNodo(xmlDoc, raiz, ns, "tipoFactura", factura.TipoFactura.ToString());
        AgregarNodo(xmlDoc, raiz, ns, "tipoDocumentoSector", factura.TipoDocumentoSector.ToString());
        AgregarNodo(xmlDoc, raiz, ns, "codigoSucursal", factura.CodigoSucursal.ToString());

        // Punto de venta: omitir el nodo si es null (regla SIN: sin nodos vacíos)
        if (factura.CodigoPuntoVenta.HasValue)
            AgregarNodo(xmlDoc, raiz, ns, "codigoPuntoVenta", factura.CodigoPuntoVenta.Value.ToString());

        AgregarNodo(xmlDoc, raiz, ns, "numeroFactura", factura.NumeroFactura.ToString());
        AgregarNodo(xmlDoc, raiz, ns, "cuf", factura.Cuf);
        AgregarNodo(xmlDoc, raiz, ns, "cufd", factura.Cufd);

        // Fecha ISO 8601 con milisegundos — requerido por el SIN
        AgregarNodo(xmlDoc, raiz, ns, "fechaEmision",
            factura.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss.fff"));

        // ── Datos del cliente ──
        AgregarNodo(xmlDoc, raiz, ns, "codigoTipoDocumentoIdentidad",
            factura.CodigoTipoDocumentoIdentidad.ToString());
        AgregarNodo(xmlDoc, raiz, ns, "numeroDocumento", factura.NumeroDocumento);
        AgregarNodo(xmlDoc, raiz, ns, "nombreRazonSocial", factura.NombreRazonSocial);

        // Complemento: omitir si es null o vacío
        if (!string.IsNullOrWhiteSpace(factura.Complemento))
            AgregarNodo(xmlDoc, raiz, ns, "complemento", factura.Complemento);

        // ── Importes ──
        // Decimales con formato invariant (punto como separador) — NUNCA usar cultura boliviana aquí
        AgregarNodo(xmlDoc, raiz, ns, "montoTotal",
            factura.MontoTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        AgregarNodo(xmlDoc, raiz, ns, "montoTotalSujetoIva",
            factura.MontoTotalSujetoIva.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        AgregarNodo(xmlDoc, raiz, ns, "codigoMoneda", "1"); // 1 = Boliviano (constante SIN)
        AgregarNodo(xmlDoc, raiz, ns, "tipoCambio", "1.00"); // 1.00 para Bs (sin conversión)
        AgregarNodo(xmlDoc, raiz, ns, "montoTotalMoneda",
            factura.MontoTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        AgregarNodo(xmlDoc, raiz, ns, "codigoMetodoPago", factura.CodigoMetodoPago.ToString());

        // ── Leyenda Ley 453 ──
        AgregarNodo(xmlDoc, raiz, ns, "leyenda", factura.Leyenda);

        // ── Detalle de ítems ──
        foreach (var detalle in factura.Details)
        {
            var itemNodo = xmlDoc.CreateElement("detalleFactura", ns);
            raiz.AppendChild(itemNodo);

            AgregarNodo(xmlDoc, itemNodo, ns, "actividadEconomica", detalle.ActividadEconomica);
            AgregarNodo(xmlDoc, itemNodo, ns, "codigoProductoSin", detalle.CodigoProductoSin.ToString());
            AgregarNodo(xmlDoc, itemNodo, ns, "codigoProducto", detalle.CodigoProducto);
            AgregarNodo(xmlDoc, itemNodo, ns, "descripcion", detalle.Descripcion);
            AgregarNodo(xmlDoc, itemNodo, ns, "cantidad",
                detalle.Cantidad.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            AgregarNodo(xmlDoc, itemNodo, ns, "unidadMedida", detalle.UnidadMedida.ToString());
            AgregarNodo(xmlDoc, itemNodo, ns, "precioUnitario",
                detalle.PrecioUnitario.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            AgregarNodo(xmlDoc, itemNodo, ns, "subTotal",
                detalle.SubTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return xmlDoc;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // HELPERS PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Agrega un nodo XML solo si el valor no es nulo ni vacío.
    /// Implementa la regla SIAT: "nodos vacíos están prohibidos y causan rechazo".
    /// </summary>
    private static void AgregarNodo(
        XmlDocument doc,
        XmlElement padre,
        string ns,
        string nombre,
        string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return; // No crear nodo vacío

        var nodo = doc.CreateElement(nombre, ns);
        nodo.InnerText = valor;
        padre.AppendChild(nodo);
    }

    /// <summary>
    /// Serializa el XmlDocument a string UTF-8 con formato limpio.
    /// </summary>
    private static string SerializarXml(XmlDocument xmlDoc)
    {
        using var ms = new MemoryStream();
        using var writer = new XmlTextWriter(ms, Encoding.UTF8)
        {
            Formatting = Formatting.Indented
        };
        xmlDoc.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
