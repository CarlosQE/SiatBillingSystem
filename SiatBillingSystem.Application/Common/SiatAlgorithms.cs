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
    // ── MÓDULO 11 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula el dígito de control usando el algoritmo Módulo 11 especificado por el SIN.
    /// Retorna la cadena COMPLETA (entrada + dígito de control), no solo el dígito.
    /// </summary>
    /// <param name="cadena">Cadena numérica de entrada.</param>
    /// <param name="limite">Límite superior del multiplicador antes de reiniciar (normalmente 9).</param>
    /// <param name="x10">
    ///     Si es TRUE: usa la variante (s*10) % 11 — donde residuo 10 → "0".
    ///     Si es FALSE: usa la variante s % 11 — donde residuo 10 → "1", residuo 11 → "0".
    ///     El SIN usa x10=FALSE para el CUF.
    /// </param>
    /// <returns>La cadena original más el dígito de control al final.</returns>
    public static string ObtenerModulo11(string cadena, int limite, bool x10)
    {
        int multiplicador = 1;
        int suma = 0;

        for (int i = cadena.Length - 1; i >= 0; i--)
        {
            multiplicador++;
            if (multiplicador > limite) multiplicador = 2;
            suma += int.Parse(cadena[i].ToString()) * multiplicador;
        }

        string digitoControl;
        if (x10)
        {
            int residuo = (suma * 10) % 11;
            digitoControl = residuo == 10 ? "0" : residuo.ToString();
        }
        else
        {
            int residuo = suma % 11;
            digitoControl = residuo switch
            {
                10 => "1",
                11 => "0",
                _  => residuo.ToString()
            };
        }

        // Retorna cadena completa + dígito de control (Base16 necesita el número entero)
        return cadena + digitoControl;
    }

    // ── BASE 16 ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Convierte una cadena numérica entera a su representación hexadecimal en MAYÚSCULAS.
    /// Se usa BigInteger porque la cadena del CUF supera el rango de long/ulong.
    /// </summary>
    public static string Base16(string cadena)
    {
        if (!BigInteger.TryParse(cadena, out BigInteger valor))
            throw new ArgumentException($"La cadena '{cadena}' no es un número entero válido para conversión Base16.");

        // valor.ToString("X") puede agregar un "0" de signo si el bit más alto es 1.
        // Lo eliminamos para obtener el hex limpio.
        var hex = valor.ToString("X").TrimStart('0');
        return string.IsNullOrEmpty(hex) ? "0" : hex;
    }

    // ── GENERACIÓN DEL CUF ────────────────────────────────────────────────────

    /// <summary>
    /// Genera el Código Único de Factura (CUF) según la especificación técnica del SIAT.
    ///
    /// FÓRMULA OFICIAL:
    ///   CUF = Base16( Módulo11( NIT + FechaHora + Sucursal + Modalidad + TipoEmision +
    ///                           TipoFactura + TipoDocSector + NumFactura + PuntoVenta ) )
    ///
    /// Campos y padding:
    ///   NIT          → sin padding (longitud variable)
    ///   FechaHora    → yyyyMMddHHmmssfff (17 dígitos)
    ///   Sucursal     → 4 dígitos, PadLeft '0'
    ///   Modalidad    → sin padding
    ///   TipoEmision  → sin padding
    ///   TipoFactura  → sin padding
    ///   TipoDocSector→ sin padding
    ///   NumFactura   → 10 dígitos, PadLeft '0'
    ///   PuntoVenta   → 4 dígitos, PadLeft '0'
    /// </summary>
    public static string GenerarCUF(ServiceInvoice factura)
    {
        // Paso 1: Construir cadena numérica en el orden exacto del SIN
        string cadenaNumerica =
            factura.NitEmisor +
            factura.FechaEmision.ToString("yyyyMMddHHmmssfff") +
            factura.CodigoSucursal.ToString().PadLeft(4, '0') +
            factura.CodigoModalidad.ToString() +
            factura.TipoEmision.ToString() +
            factura.TipoFactura.ToString() +
            factura.TipoDocumentoSector.ToString() +
            factura.NumeroFactura.ToString().PadLeft(10, '0') +
            (factura.CodigoPuntoVenta ?? 0).ToString().PadLeft(4, '0');

        // Paso 2: Aplicar Módulo 11 (variante SIN: límite=9, x10=false)
        // Retorna la cadena completa con el dígito de control al final
        string cadenaConControl = ObtenerModulo11(cadenaNumerica, limite: 9, x10: false);

        // Paso 3: Convertir el número completo a hexadecimal
        return Base16(cadenaConControl);
    }

    // ── VERIFICACIÓN DEL CUF ──────────────────────────────────────────────────

    /// <summary>
    /// Verifica que un CUF dado sea consistente con los datos de una factura.
    /// Útil para validar facturas antes de reenviarlas al SIN o en procesos de auditoría.
    /// </summary>
    public static bool VerificarCUF(ServiceInvoice factura, string cufAVerificar)
    {
        if (string.IsNullOrWhiteSpace(cufAVerificar)) return false;
        return string.Equals(GenerarCUF(factura), cufAVerificar, StringComparison.OrdinalIgnoreCase);
    }
}