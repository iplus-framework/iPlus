using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    public class ConverterDataType : IValueConverter
    {
        private static ConverterDataType _Current;

        public static ConverterDataType Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new ConverterDataType();
                return _Current;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

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
