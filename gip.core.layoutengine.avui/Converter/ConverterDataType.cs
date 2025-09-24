using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    public class ConverterDataType : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            return value.GetType();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
