using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SiatBillingSystem.Desktop.Converters;

public class BoolToWidthConverter : IValueConverter
{
    public double TrueValue { get; set; } = 220;
    public double FalseValue { get; set; } = 60;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isTrue = value is bool b && b;
        double width = isTrue ? TrueValue : FalseValue;

        if (targetType == typeof(GridLength))
            return new GridLength(width);

        return width;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NullToNewEditTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is null ? "NUEVO CLIENTE" : "EDITAR CLIENTE";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}