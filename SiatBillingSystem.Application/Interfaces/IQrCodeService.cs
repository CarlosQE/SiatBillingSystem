namespace SiatBillingSystem.Application.Interfaces;

/// <summary>
/// Servicio para generación de códigos QR según estándares del SIN Bolivia.
/// </summary>
public interface IQrCodeService
{
    /// <summary>
    /// Genera la URL de verificación oficial del SIN Bolivia.
    /// Formato: https://siat.impuestos.gob.bo/consulta/QR?nit={nit}&amp;cuf={cuf}&amp;numero={numero}
    /// </summary>
    string GenerarUrlVerificacion(string nit, string cuf, long numeroFactura);

    /// <summary>Genera el QR como array de bytes PNG listo para insertar en el PDF.</summary>
    byte[] GenerarQrPng(string contenido, int pixelesPorModulo = 5);
}
