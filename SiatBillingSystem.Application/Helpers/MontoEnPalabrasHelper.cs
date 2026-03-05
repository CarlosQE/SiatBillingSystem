namespace SiatBillingSystem.Application.Helpers;

/// <summary>
/// Convierte montos decimales a texto en español para impresión en facturas.
/// Ejemplo: 1234.56 → "MIL DOSCIENTOS TREINTA Y CUATRO CON 56/100 BOLIVIANOS"
/// </summary>
public static class MontoEnPalabrasHelper
{
    private static readonly string[] Unidades =
    {
        "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
        "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE",
        "DIECIOCHO", "DIECINUEVE"
    };

    private static readonly string[] Decenas =
    {
        "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA",
        "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
    };

    private static readonly string[] Centenas =
    {
        "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS",
        "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
    };

    public static string Convertir(decimal monto)
    {
        if (monto < 0) return "MONTO INVÁLIDO";

        var parteEntera = (long)Math.Floor(monto);
        var centavos = (int)Math.Round((monto - parteEntera) * 100);

        var palabras = parteEntera == 0
            ? "CERO"
            : ConvertirEntero(parteEntera);

        return $"{palabras} CON {centavos:D2}/100 BOLIVIANOS";
    }

    private static string ConvertirEntero(long numero)
    {
        if (numero == 0) return "";
        if (numero == 1) return "UN";
        if (numero < 20) return Unidades[numero];
        if (numero == 100) return "CIEN";

        if (numero < 100)
        {
            var decena = Decenas[numero / 10];
            var unidad = numero % 10 == 0 ? "" : $" Y {Unidades[numero % 10]}";
            return $"{decena}{unidad}";
        }

        if (numero < 1_000)
        {
            var centena = Centenas[numero / 100];
            var resto = numero % 100 == 0 ? "" : $" {ConvertirEntero(numero % 100)}";
            return $"{centena}{resto}";
        }

        if (numero < 1_000_000)
        {
            var miles = numero / 1_000;
            var milesTexto = miles == 1 ? "MIL" : $"{ConvertirEntero(miles)} MIL";
            var resto = numero % 1_000 == 0 ? "" : $" {ConvertirEntero(numero % 1_000)}";
            return $"{milesTexto}{resto}";
        }

        if (numero < 1_000_000_000)
        {
            var millones = numero / 1_000_000;
            var millonesTexto = millones == 1
                ? "UN MILLÓN"
                : $"{ConvertirEntero(millones)} MILLONES";
            var resto = numero % 1_000_000 == 0 ? "" : $" {ConvertirEntero(numero % 1_000_000)}";
            return $"{millonesTexto}{resto}";
        }

        return numero.ToString();
    }
}
