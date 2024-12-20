﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace gip.core.layoutengine.timeline
{
    public class AddDoubleConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = System.Convert.ToDouble(value);
            double addVal = System.Convert.ToDouble(parameter);

            return (val + addVal);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class DoubleToTimeSpanFromMinutesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double minutes = (double)value;
            return TimeSpan.FromSeconds(minutes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan ts = (TimeSpan)value;
            return ts.TotalSeconds;
        }
    }
}
