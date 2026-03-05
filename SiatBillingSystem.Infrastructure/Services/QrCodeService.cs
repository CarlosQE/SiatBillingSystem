using QRCoder;
using SiatBillingSystem.Application.Interfaces;

namespace SiatBillingSystem.Infrastructure.Services;

/// <summary>
/// Generación de códigos QR para facturas SIAT Bolivia.
/// Usa la librería QRCoder (MIT License) — sin dependencias de GDI+ en versiones modernas.
/// </summary>
public class QrCodeService : IQrCodeService
{
    /// <summary>
    /// URL oficial de verificación del SIN Bolivia para el código QR de la factura.
    /// El SIN escanea este URL para validar la autenticidad de la factura.
    /// </summary>
    public string GenerarUrlVerificacion(string nit, string cuf, long numeroFactura)
    {
        return $"https://siat.impuestos.gob.bo/consulta/QR?nit={nit}&cuf={cuf}&numero={numeroFactura}";
    }

    /// <summary>
    /// Genera el código QR como imagen PNG en bytes.
    /// Usa PngByteQRCode para compatibilidad con Linux/Mac sin GDI+.
    /// </summary>
    public byte[] GenerarQrPng(string contenido, int pixelesPorModulo = 5)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            return Array.Empty<byte>();

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(pixelesPorModulo);
    }
}
