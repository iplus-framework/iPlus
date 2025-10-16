using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// The converter for scroll bar size.
    /// </summary>
    //[ValueConversion(typeof(double), typeof(double))]
    public class VBScrollBarSizeConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((values == null) /*|| (parameter == null)*/)
                return (double)2000;
            if ((values[0] == null) || (values[1] == null) || (values[2] == null) || (values[3] == null))
                return (double)2000;

            double viewportSize = (double)values[0];
            double maximum = (double)values[1];
            double minimum = (double)values[2];
            double trackLength = (double)values[3];
            double thumbSize = (viewportSize / (maximum - minimum + viewportSize)) * trackLength;
            return thumbSize;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class DoubleIsNaNConverter : IValueConverter
    {
        public static readonly DoubleIsNaNConverter IsNaN = new DoubleIsNaNConverter(true);
        public static readonly DoubleIsNaNConverter IsNotNaN = new DoubleIsNaNConverter(false);


        readonly bool _equalTo;
        private DoubleIsNaNConverter(bool equalTo)
            => _equalTo = equalTo;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double val))
                return _equalTo;

            return (double.IsNaN(val) || double.IsInfinity(val)) == _equalTo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
