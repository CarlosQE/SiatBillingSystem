using System.Security.Cryptography;
using System.Text;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
namespace SiatBillingSystem.Infrastructure.Security;

/// <summary>
/// Servicio de protección de datos usando Windows DPAPI (Data Protection API).
///
/// DPAPI cifra datos ligándolos a la cuenta de Windows o a la máquina.
/// La contraseña del certificado digital .p12/.pfx se cifra con DPAPI
/// para que sea ilegible incluso si alguien copia el archivo de base de datos.
///
/// Limitación: Solo funciona en Windows. Para Linux/Mac se usaría
/// Microsoft.AspNetCore.DataProtection como alternativa multiplataforma.
/// Para On-Premise (Windows), DPAPI es la opción más segura y nativa.
/// </summary>
public static class DpapiProtector
{
    /// <summary>
    /// Cifra un texto plano usando DPAPI con scope de usuario actual.
    /// El resultado es Base64 para almacenamiento en BD.
    /// </summary>
    public static string Cifrar(string textoPLano)
    {
        if (string.IsNullOrWhiteSpace(textoPLano))
            throw new ArgumentException("No se puede cifrar un texto vacío.");

        var bytes = Encoding.UTF8.GetBytes(textoPLano);
        var bytesCifrados = ProtectedData.Protect(
            bytes,
            optionalEntropy: null,
            scope: DataProtectionScope.CurrentUser); // Solo el mismo usuario de Windows puede descifrar

        return Convert.ToBase64String(bytesCifrados);
    }

    /// <summary>
    /// Descifra un texto cifrado con DPAPI previamente.
    /// Lanza excepción si el usuario de Windows no es el mismo que cifró.
    /// </summary>
    public static string Descifrar(string base64Cifrado)
    {
        if (string.IsNullOrWhiteSpace(base64Cifrado))
            throw new ArgumentException("El texto cifrado no puede estar vacío.");

        try
        {
            var bytesCifrados = Convert.FromBase64String(base64Cifrado);
            var bytesDescifrados = ProtectedData.Unprotect(
                bytesCifrados,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(bytesDescifrados);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException(
                "No se pudo descifrar la contraseña del certificado. " +
                "Esto ocurre si el sistema fue movido a otra máquina o usuario de Windows. " +
                "Vaya a Configuración y vuelva a ingresar la contraseña del certificado.", ex);
        }
    }
}
