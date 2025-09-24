using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace gip.core.layoutengine.avui
{
    public class ConverterIsNotEqual : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value.ToString().Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
