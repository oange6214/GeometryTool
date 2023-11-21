using System;
using System.Globalization;
using System.Windows.Data;

namespace MainGui.Converters;
public class DoubleFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var d = (double)value;
        return Math.Round(d);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}