using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace gip.core.layoutengine.avui
{
    public class ConverterIsNotEqual : IValueConverter
    {
        private static ConverterIsNotEqual _Current;

        public static ConverterIsNotEqual Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new ConverterIsNotEqual();
                return _Current;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

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
