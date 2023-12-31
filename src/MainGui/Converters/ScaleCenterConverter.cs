﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace MainGui.Converters;

internal class ScaleCenterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((double)value > 0)
        {
            return (double)value / 2.0;
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}