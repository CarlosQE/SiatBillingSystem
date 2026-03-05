using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace SiatBillingSystem.Application.Interfaces;

/// <summary>
/// Contrato para el servicio de firma digital XML (DSIG).
/// Implementado en Infrastructure usando System.Security.Cryptography.Xml.
/// </summary>
public interface ISignatureService
{
    /// <summary>
    /// Carga el certificado digital desde un archivo .p12 o .pfx.
    /// </summary>
    X509Certificate2 CargarCertificado(string rutaCertificado, string password);

    /// <summary>
    /// Aplica firma digital XML (Enveloped Signature + C14N) sobre el XmlDocument.
    /// Modifica el documento in-place inyectando el nodo Signature.
    /// </summary>
    void FirmarXml(XmlDocument xmlDoc, X509Certificate2 certificado);
}
