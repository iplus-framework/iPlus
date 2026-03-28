using Avalonia.Data.Converters;
using System;

namespace gip.core.layoutengine.avui
{
    public class ConverterDoubleOffset : IValueConverter
    {
        private static ConverterDoubleOffset _Current;

        public static ConverterDoubleOffset Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new ConverterDoubleOffset();
                return _Current;
            }
        }
        
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

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
