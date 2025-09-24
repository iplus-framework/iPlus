using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    public class InverseBoolConverter : IValueConverter
    {
        private static InverseBoolConverter _Current;

        public static InverseBoolConverter Current
        {
            get
            {
                if (_Current == null) _Current = new InverseBoolConverter();
                return _Current;
            }
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        #endregion
    }

    //[ValueConversion(typeof(object), typeof(bool))]
    public class NullConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

}
