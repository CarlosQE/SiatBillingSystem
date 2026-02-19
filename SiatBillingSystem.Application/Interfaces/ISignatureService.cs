using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace SiatBillingSystem.Application.Interfaces;

/// <summary>
/// Contrato para el servicio de Firma Digital XML según estándar W3C DSIG.
/// Implementado en la capa Infrastructure para mantener el Dominio y Application limpios
/// de dependencias criptográficas del sistema operativo.
/// </summary>
public interface ISignatureService
{
    /// <summary>
    /// Aplica una firma digital XML (Enveloped Signature) al documento proporcionado.
    /// La firma se inyecta como último nodo hijo del elemento raíz del XML.
    /// </summary>
    /// <param name="xmlDoc">Documento XML a firmar. Se modifica en lugar (in-place).</param>
    /// <param name="certificate">Certificado X.509 con clave privada RSA (.p12 / .pfx).</param>
    void FirmarXml(XmlDocument xmlDoc, X509Certificate2 certificate);

    /// <summary>
    /// Carga un certificado digital desde un archivo .p12 o .pfx en disco.
    /// </summary>
    /// <param name="rutaArchivo">Ruta absoluta al archivo del certificado.</param>
    /// <param name="password">Contraseña del certificado.</param>
    /// <returns>Certificado X.509 listo para firmar.</returns>
    X509Certificate2 CargarCertificado(string rutaArchivo, string password);

    /// <summary>
    /// Verifica que la firma digital de un documento XML sea válida.
    /// Útil para validar facturas recibidas antes de procesarlas.
    /// </summary>
    /// <param name="xmlDoc">Documento XML con firma embebida.</param>
    /// <returns>TRUE si la firma es criptográficamente válida.</returns>
    bool VerificarFirma(XmlDocument xmlDoc);
}
