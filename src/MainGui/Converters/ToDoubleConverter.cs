using System;
using System.Globalization;
using System.Windows.Data;

namespace MainGui.Converters;

internal class ToDoubleConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not null)
        {
            var sliderValue = (double)value;
            if (sliderValue <= 1)
            {
                return sliderValue * 10;
            }
            return sliderValue;
        }
        return null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not null)
        {
            var sliderValue = (double)value;
            if (sliderValue < 1)
            {
                return sliderValue;
            }

            return System.Convert.ToDouble(sliderValue / 10);
        }
        return null;
    }
}
