using SiatBillingSystem.Domain.Entities;

namespace SiatBillingSystem.Application.Interfaces;

/// <summary>
/// Servicio para generación de representación gráfica (PDF) de facturas SIAT.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Genera el PDF de la factura y retorna los bytes.
    /// </summary>
    byte[] GenerarPdf(ServiceInvoice factura, ConfiguracionEmpresa empresa, byte[] qrImageBytes);

    /// <summary>
    /// Guarda el PDF en la carpeta de facturas del usuario y retorna la ruta completa.
    /// </summary>
    string GuardarPdf(ServiceInvoice factura, ConfiguracionEmpresa empresa, byte[] qrImageBytes);

    /// <summary>
    /// Abre el PDF con el visor predeterminado de Windows.
    /// </summary>
    void AbrirPdf(string rutaPdf);
}
