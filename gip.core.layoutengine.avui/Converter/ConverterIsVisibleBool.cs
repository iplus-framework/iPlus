using Avalonia.Data.Converters;
using System;
using System.Globalization;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui
{
    [Converter(typeof(object), typeof(bool))]
    public class ConverterIsVisibleBool : IValueConverter
    {
        private static ConverterIsVisibleBool _Current;

        public static ConverterIsVisibleBool Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new ConverterIsVisibleBool();
                return _Current;
            }
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = false;
            if (value != null)
            {
                if (value is string s)
                    bool.TryParse(s, out visible);
                else if (value is IConvertible)
                    visible = Convert.ToBoolean(value);
                else if (value is bool b)
                    visible = b;
            }
            return visible;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }
    }
}
