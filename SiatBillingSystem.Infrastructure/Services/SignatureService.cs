using System;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using SiatBillingSystem.Infrastructure.Interfaces;

namespace SiatBillingSystem.Infrastructure.Services
{
    public class SignatureService : ISignatureService
    {
        public void FirmarXml(XmlDocument xmlDoc, X509Certificate2 certificate)
        {
            if (xmlDoc.DocumentElement == null)
                throw new ArgumentException("El documento XML no tiene un elemento raíz.");

            // 1. Crear el objeto SignedXml
            SignedXml signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = certificate.GetRSAPrivateKey()
            };

            // 2. Configurar la referencia (lo que se va a firmar)
            Reference reference = new Reference();
            reference.Uri = ""; // Firma todo el documento

            // 3. Añadir transformaciones (Requerido por SIAT para normalizar el XML)
            XmlDsigEnvelopedSignatureTransform envelopedTransform = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(envelopedTransform);

            XmlDsigC14NTransform c14nTransform = new XmlDsigC14NTransform();
            reference.AddTransform(c14nTransform);

            signedXml.AddReference(reference);

            // 4. Configurar información del certificado (KeyInfo)
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate));
            signedXml.KeyInfo = keyInfo;

            // 5. Calcular la firma
            signedXml.ComputeSignature();

            // 6. Obtener la representación XML de la firma y añadirla al documento
            XmlElement xmlDigitalSignature = signedXml.GetXml();
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        }
    }
}