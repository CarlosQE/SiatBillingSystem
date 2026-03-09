using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SiatBillingSystem.Desktop.Converters
{
    /// <summary>
    /// Convierte un string de color hex (ej: "#f59e0b") a SolidColorBrush.
    /// Usado en el badge de estado del historial de facturas.
    /// </summary>
    public class HexColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch { /* hex inválido — caer al fallback */ }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}