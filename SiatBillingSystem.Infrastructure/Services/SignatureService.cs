using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using SiatBillingSystem.Infrastructure.Interfaces;

namespace SiatBillingSystem.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de Firma Digital XML según estándar W3C XML-DSIG.
///
/// El SIAT exige específicamente:
///   1. Algoritmo de firma: RSA con SHA-256 (o SHA-1 según versión del certificado ADSIB)
///   2. Tipo: Enveloped Signature (la firma se inyecta DENTRO del XML, no en sobre externo)
///   3. Transformaciones: XmlDsigEnvelopedSignatureTransform + XmlDsigC14NTransform (canonicalización)
///   4. KeyInfo: Debe incluir el certificado X.509 completo (KeyInfoX509Data)
///
/// Certificados aceptados por el SIN: ADSIB (.p12) o DigiCert (.pfx)
/// </summary>
public class SignatureService : ISignatureService
{
    // ─────────────────────────────────────────────────────────────────────────────
    // FIRMA DIGITAL
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Aplica firma digital XML Enveloped al documento.
    /// La firma se añade como último hijo del elemento raíz, modificando el documento in-place.
    /// </summary>
    public void FirmarXml(XmlDocument xmlDoc, X509Certificate2 certificate)
    {
        if (xmlDoc.DocumentElement is null)
            throw new ArgumentException("El documento XML no tiene elemento raíz. No se puede firmar.");

        var clavePrivada = certificate.GetRSAPrivateKey()
            ?? throw new InvalidOperationException(
                "El certificado no contiene clave privada RSA. " +
                "Verifique que el archivo .p12/.pfx sea el certificado del EMISOR (no solo el público).");

        // ── 1. Crear el objeto de firma vinculado al documento completo ──
        var signedXml = new SignedXml(xmlDoc)
        {
            SigningKey = clavePrivada
        };

        // ── 2. Definir qué se firma: URI="" significa el documento completo ──
        var reference = new Reference { Uri = "" };

        // ── 3. Transformación 1: Enveloped — excluye el propio nodo <Signature> del hash
        //       (evita referencia circular al firmarse a sí mismo)
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());

        // ── 4. Transformación 2: C14N — Canonicalización del XML antes de calcular el hash
        //       Normaliza espacios, orden de atributos y encoding para que el hash sea determinista
        reference.AddTransform(new XmlDsigC14NTransform());

        signedXml.AddReference(reference);

        // ── 5. KeyInfo: incluir el certificado público completo en la firma
        //       El SIN lo usa para verificar la identidad del emisor sin consulta externa
        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificate));
        signedXml.KeyInfo = keyInfo;

        // ── 6. Calcular la firma (RSA sobre el hash SHA del XML canonicalizado) ──
        signedXml.ComputeSignature();

        // ── 7. Obtener el nodo XML <Signature> e inyectarlo en el documento ──
        var xmlSignature = signedXml.GetXml();
        xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlSignature, deep: true));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // CARGA DE CERTIFICADO
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Carga un certificado digital desde archivo .p12 o .pfx.
    ///
    /// Seguridad: Este método NO almacena la contraseña en memoria más allá de la carga.
    /// Para producción (On-Premise), la contraseña debe venir de variables de entorno
    /// o del Windows DPAPI — nunca hardcodeada en el código fuente.
    /// </summary>
    public X509Certificate2 CargarCertificado(string rutaArchivo, string password)
    {
        if (!File.Exists(rutaArchivo))
            throw new FileNotFoundException(
                $"No se encontró el certificado digital en: '{rutaArchivo}'. " +
                "Verifique la ruta configurada en los ajustes de la aplicación.");

        var extension = Path.GetExtension(rutaArchivo).ToLowerInvariant();
        if (extension is not (".p12" or ".pfx"))
            throw new ArgumentException(
                $"Formato de certificado no soportado: '{extension}'. " +
                "El SIN acepta únicamente archivos .p12 (ADSIB) o .pfx (DigiCert).");

        try
        {
            // X509KeyStorageFlags.Exportable: necesario para extraer la clave privada RSA
            // X509KeyStorageFlags.PersistKeySet: evita que la clave se destruya al salir del scope
            var certificate = new X509Certificate2(
                rutaArchivo,
                password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

            // Validar que el certificado tiene clave privada (requerido para firmar)
            if (!certificate.HasPrivateKey)
                throw new InvalidOperationException(
                    "El certificado cargado no contiene clave privada. " +
                    "Para firmar facturas se necesita el certificado COMPLETO (.p12/.pfx), no solo el certificado público (.cer/.crt).");

            // Validar vigencia del certificado
            var ahora = DateTime.Now;
            if (ahora < certificate.NotBefore || ahora > certificate.NotAfter)
                throw new InvalidOperationException(
                    $"El certificado digital está VENCIDO o aún no es válido. " +
                    $"Vigente desde: {certificate.NotBefore:dd/MM/yyyy} hasta: {certificate.NotAfter:dd/MM/yyyy}. " +
                    $"Fecha actual: {ahora:dd/MM/yyyy}. Contacte a ADSIB para renovarlo.");

            return certificate;
        }
        catch (Exception ex) when (ex is not FileNotFoundException
                                       and not ArgumentException
                                       and not InvalidOperationException)
        {
            // Captura errores de contraseña incorrecta u otros errores criptográficos del SO
            throw new InvalidOperationException(
                "Error al cargar el certificado. Posibles causas: " +
                "contraseña incorrecta, archivo corrupto o formato no compatible. " +
                $"Detalle técnico: {ex.Message}", ex);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // VERIFICACIÓN DE FIRMA
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica la integridad y autenticidad de la firma digital en un XML.
    /// Confirma que el documento no fue alterado después de ser firmado.
    /// </summary>
    public bool VerificarFirma(XmlDocument xmlDoc)
    {
        if (xmlDoc.DocumentElement is null)
            return false;

        // Buscar el nodo <Signature> embebido en el documento
        var nodeList = xmlDoc.GetElementsByTagName(
            "Signature",
            "http://www.w3.org/2000/09/xmldsig#");

        if (nodeList.Count == 0)
            return false; // No hay firma embebida

        var signedXml = new SignedXml(xmlDoc);
        signedXml.LoadXml((XmlElement)nodeList[0]!);

        // CheckSignature sin parámetros verifica usando la clave pública embebida en KeyInfo
        return signedXml.CheckSignature();
    }
}
