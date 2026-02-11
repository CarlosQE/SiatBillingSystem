using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace SiatBillingSystem.Infrastructure.Interfaces
{
    public interface ISignatureService
    {
        /// <summary>
        /// Firma un documento XML siguiendo el estándar DSIG requerido por el SIAT.
        /// </summary>
        void FirmarXml(XmlDocument xmlDoc, X509Certificate2 certificate);
    }
}