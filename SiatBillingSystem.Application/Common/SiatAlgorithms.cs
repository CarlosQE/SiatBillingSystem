using System;
using System.Text;
using System.Numerics;

namespace SiatBillingSystem.Application.Common
{
    public static class SiatAlgorithms
    {
        /// <summary>
        /// Algoritmo Módulo 11 solicitado por el SIN para la generación de dígitos de control.
        /// </summary>
        public static string ObtenerModulo11(string cadena, int numDig, int limite, bool x10)
        {
            int m, s, i, j;
            if (!x10) numDig = 1;

            for (int n = 1; n <= numDig; n++)
            {
                m = 1;
                s = 0;
                for (i = cadena.Length - 1; i >= 0; i--)
                {
                    m++;
                    if (m > limite) m = 2;
                    s += int.Parse(cadena[i].ToString()) * m;
                }

                if (x10)
                {
                    int p = (s * 10) % 11;
                    if (p == 10) cadena += "0";
                    else cadena += p.ToString();
                }
                else
                {
                    int p = s % 11;
                    if (p == 10) cadena += "1";
                    else if (p == 11) cadena += "0";
                    else cadena += p.ToString();
                }
            }
            return cadena.Substring(cadena.Length - numDig);
        }

        /// <summary>
        /// Convierte una cadena numérica a Base 16 (Hexadecimal) en mayúsculas.
        /// </summary>
        public static string Base16(string cadena)
        {
            BigInteger valor = BigInteger.Parse(cadena);
            return valor.ToString("X");
        }
    }
}