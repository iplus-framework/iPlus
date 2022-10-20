using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace gip.core.layoutengine
{
    public class ConverterVisibilityInverseBool : IValueConverter
    {
        private static ConverterVisibilityInverseBool _Current;

        public static ConverterVisibilityInverseBool Current
        {
            get
            {
                if (_Current == null) _Current = new ConverterVisibilityInverseBool();
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
            if (!visible)
                visibility = Visibility.Visible;
            return visibility;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
