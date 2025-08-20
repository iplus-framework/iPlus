using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Reflection;
using System.Windows;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    public class ConverterVisibilityBool : IValueConverter
    {
        private static ConverterVisibilityBool _Current;

        public static ConverterVisibilityBool Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new ConverterVisibilityBool();
                return _Current;
            }
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Collapsed;
            if (parameter != null)
                visibility = Visibility.Hidden;
            bool visible = false;
            if (value != null)
            {
                if (value is String)
                    bool.TryParse((String)value, out visible);
                else if (value is IConvertible)
                    visible = System.Convert.ToBoolean(value);
            }
            if (visible)
                visibility = Visibility.Visible;
            return visibility;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
