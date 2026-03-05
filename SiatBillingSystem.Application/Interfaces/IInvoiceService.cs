using SiatBillingSystem.Domain.Entities;

namespace SiatBillingSystem.Application.Interfaces;

/// <summary>
/// Contrato para el servicio principal de facturación.
/// Orquesta la generación del CUF, construcción del XML y preparación para envío al SIAT.
///
/// Esta interfaz vive en Application — la capa que conoce las reglas de negocio
/// pero NO sabe nada de bases de datos, archivos ni servicios web externos.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Prepara una factura completa lista para enviar al SIAT:
    /// calcula el CUF, valida los datos y genera el XML firmado.
    /// </summary>
    /// <param name="factura">Datos de la factura a procesar.</param>
    /// <param name="rutaCertificado">Ruta al archivo .p12/.pfx del certificado digital.</param>
    /// <param name="passwordCertificado">Contraseña del certificado digital.</param>
    /// <returns>Resultado con el XML firmado y el CUF generado.</returns>
    Task<InvoiceResult> PrepararFacturaAsync(
        ServiceInvoice factura,
        string rutaCertificado,
        string passwordCertificado);

    /// <summary>
    /// Calcula y asigna el CUF a una factura sin generar el XML.
    /// Útil para pre-visualización antes de confirmar la emisión.
    /// </summary>
    /// <param name="factura">Factura con todos los datos requeridos.</param>
    /// <returns>CUF calculado en hexadecimal.</returns>
    string CalcularCuf(ServiceInvoice factura);
}

/// <summary>
/// Resultado de la preparación de una factura.
/// Encapsula el XML firmado y metadata del proceso.
/// </summary>
public class InvoiceResult
{
    /// <summary>Indica si el proceso fue exitoso.</summary>
    public bool Exitoso { get; init; }

    /// <summary>CUF generado para esta factura.</summary>
    public string Cuf { get; init; } = string.Empty;

    /// <summary>XML firmado listo para enviar al endpoint SOAP del SIN.</summary>
    public string XmlFirmado { get; init; } = string.Empty;

    /// <summary>Mensaje de error en caso de fallo. Null si fue exitoso.</summary>
    public string? Error { get; init; }

    /// <summary>Instancia de éxito.</summary>
    public static InvoiceResult Ok(string cuf, string xmlFirmado) =>
        new() { Exitoso = true, Cuf = cuf, XmlFirmado = xmlFirmado };

    /// <summary>Instancia de fallo.</summary>
    public static InvoiceResult Fallo(string error) =>
        new() { Exitoso = false, Error = error };
}
