using Avalonia.Data.Converters;
using System;

namespace gip.core.layoutengine.avui
{
    public class ConverterDoubleOffset : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value + Offset;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value - Offset;
        }

        #endregion

        public double Offset { get; set; }
    }
}
