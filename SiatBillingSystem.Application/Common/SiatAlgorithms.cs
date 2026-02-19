using System.Numerics;
using SiatBillingSystem.Domain.Entities;

namespace SiatBillingSystem.Application.Common;

/// <summary>
/// Algoritmos matemáticos y criptográficos requeridos por el SIAT (SIN - Bolivia).
/// Contiene la implementación del Módulo 11, conversión Base 16 y generación del CUF.
///
/// Referencia normativa: Manual Técnico de Facturación SIAT v1.0 - Servicio de Impuestos Nacionales.
/// REGLA DE ORO: Esta clase no debe tener dependencias externas. Solo lógica pura de C#.
/// </summary>
public static class SiatAlgorithms
{
    // ─────────────────────────────────────────────────────────────────────────────
    // MÓDULO 11 — Dígito de control oficial del SIN
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula el dígito de control usando el algoritmo Módulo 11 especificado por el SIN.
    /// </summary>
    /// <param name="cadena">Cadena numérica de entrada.</param>
    /// <param name="numDig">Cantidad de dígitos de control a generar (normalmente 1).</param>
    /// <param name="limite">Límite superior del multiplicador antes de reiniciar (normalmente 9).</param>
    /// <param name="x10">
    ///     Si es TRUE: usa la variante (s*10) % 11 — donde residuo 10 → "0".
    ///     Si es FALSE: usa la variante s % 11 — donde residuo 10 → "1", residuo 11 → "0".
    ///     El SIN usa x10=FALSE para el CUF.
    /// </param>
    /// <returns>El dígito (o dígitos) de control como string.</returns>
    public static string ObtenerModulo11(string cadena, int numDig, int limite, bool x10)
    {
        if (!x10) numDig = 1;

        for (int n = 1; n <= numDig; n++)
        {
            int multiplicador = 1;
            int suma = 0;

            for (int i = cadena.Length - 1; i >= 0; i--)
            {
                multiplicador++;
                if (multiplicador > limite) multiplicador = 2;
                suma += int.Parse(cadena[i].ToString()) * multiplicador;
            }

            if (x10)
            {
                int residuo = (suma * 10) % 11;
                cadena += residuo == 10 ? "0" : residuo.ToString();
            }
            else
            {
                int residuo = suma % 11;
                cadena += residuo switch
                {
                    10 => "1",
                    11 => "0",
                    _ => residuo.ToString()
                };
            }
        }

        return cadena[^numDig..]; // Equivalente a Substring(cadena.Length - numDig), más idiomático en C# moderno
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // BASE 16 — Conversión hexadecimal para el CUF final
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Convierte una cadena numérica entera a su representación hexadecimal en MAYÚSCULAS.
    /// Se usa BigInteger porque la cadena del CUF supera el rango de long/ulong.
    /// </summary>
    /// <param name="cadena">Cadena numérica decimal (puede ser muy grande).</param>
    /// <returns>Representación hexadecimal en mayúsculas. Ej: "1A3F9C"</returns>
    public static string Base16(string cadena)
    {
        if (!BigInteger.TryParse(cadena, out BigInteger valor))
            throw new ArgumentException($"La cadena '{cadena}' no es un número entero válido para conversión Base16.");

        return valor.ToString("X");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // GENERACIÓN DEL CUF — Código Único de Factura
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Genera el Código Único de Factura (CUF) según la especificación técnica del SIAT.
    ///
    /// FÓRMULA OFICIAL:
    ///   CUF = Base16( Módulo11( NIT + FechaHora + Sucursal + Modalidad + TipoEmision +
    ///                            TipoFactura + TipoDocSector + NumFactura + PuntoVenta ) )
    ///
    /// Donde:
    ///   - FechaHora: formato yyyyMMddHHmmssfff (17 dígitos con milisegundos)
    ///   - Sucursal:  4 dígitos con cero a la izquierda (ej: 0000 para casa matriz)
    ///   - PuntoVenta: 4 dígitos con cero a la izquierda
    ///   - NumFactura: 10 dígitos con cero a la izquierda
    ///   - Módulo 11 se aplica con límite=9, x10=false (variante del SIN)
    ///   - El resultado del Módulo 11 se convierte íntegramente a Base 16
    /// </summary>
    /// <param name="factura">Entidad ServiceInvoice con todos los datos de la factura.</param>
    /// <returns>CUF en hexadecimal en mayúsculas (longitud variable, ~64 caracteres).</returns>
    public static string GenerarCUF(ServiceInvoice factura)
    {
        // ── Paso 1: Construir la cadena base numérica concatenando todos los campos ──

        // NIT del emisor (tal cual, sin padding — puede variar en longitud)
        string nit = factura.NitEmisor;

        // Fecha y hora de emisión con milisegundos: yyyyMMddHHmmssfff
        // Ejemplo: 20240115143052123 → 15 enero 2024, 14:30:52.123
        string fechaHora = factura.FechaEmision.ToString("yyyyMMddHHmmssfff");

        // Código de sucursal: 4 dígitos, rellenado con ceros a la izquierda
        // Casa matriz = 0000, Sucursal 1 = 0001, etc.
        string sucursal = factura.CodigoSucursal.ToString().PadLeft(4, '0');

        // Modalidad de facturación (catálogo SIN: 1=Electrónica en Línea, 2=Computarizada en Línea)
        string modalidad = factura.CodigoModalidad.ToString();

        // Tipo de emisión (1=Online, 2=Offline/Contingencia)
        string tipoEmision = factura.TipoEmision.ToString();

        // Tipo de factura (1=Con derecho crédito fiscal, 2=Sin derecho crédito fiscal)
        string tipoFactura = factura.TipoFactura.ToString();

        // Tipo de documento de sector (catálogo SIN: 1=Compra/Venta, 12=Servicios Turísticos, etc.)
        string tipoDocSector = factura.TipoDocumentoSector.ToString();

        // Número de factura: 10 dígitos con cero a la izquierda
        string numFactura = factura.NumeroFactura.ToString().PadLeft(10, '0');

        // Punto de venta: 4 dígitos con cero a la izquierda (0000 si no aplica)
        string puntoVenta = (factura.CodigoPuntoVenta ?? 0).ToString().PadLeft(4, '0');

        // ── Paso 2: Concatenar en el orden exacto definido por el SIN ──
        string cadenaNumerica =
            nit +
            fechaHora +
            sucursal +
            modalidad +
            tipoEmision +
            tipoFactura +
            tipoDocSector +
            numFactura +
            puntoVenta;

        // ── Paso 3: Aplicar Módulo 11 (variante SIN: límite=9, x10=false) ──
        // El resultado incluye la cadena original + 1 dígito de control al final
        string cadenaConDigitoControl = ObtenerModulo11(
            cadena: cadenaNumerica,
            numDig: 1,
            limite: 9,
            x10: false
        );

        // ── Paso 4: Convertir la cadena completa (con dígito de control) a Hexadecimal ──
        string cuf = Base16(cadenaConDigitoControl);

        return cuf;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // UTILIDAD — Validación del CUF (para verificar facturas recibidas)
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica que un CUF dado sea consistente con los datos de una factura.
    /// Útil para validar facturas antes de reenviarlas al SIN o en procesos de auditoría.
    /// </summary>
    /// <param name="factura">Datos de la factura a verificar.</param>
    /// <param name="cufAVerificar">CUF almacenado o recibido.</param>
    /// <returns>TRUE si el CUF calculado coincide con el proporcionado.</returns>
    public static bool VerificarCUF(ServiceInvoice factura, string cufAVerificar)
    {
        if (string.IsNullOrWhiteSpace(cufAVerificar)) return false;

        string cufCalculado = GenerarCUF(factura);
        return string.Equals(cufCalculado, cufAVerificar, StringComparison.OrdinalIgnoreCase);
    }
}
