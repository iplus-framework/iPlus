﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace gip.core.layoutengine.avui.AvaloniaRibbon.Converters;

/// <summary>
///     This is a converter which will add two numbers
/// </summary>
public class MathAddConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        var value = values.Sum(x => (double?)x);
        return value + (double?)parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // If we want to convert back, we need to subtract instead of add.
        return (decimal?)value - (decimal?)parameter;
    }
}