using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    public class ConverterPoint : IValueConverter, IMultiValueConverter
    {
        public string FixedX { get; set; }

        public string FixedY { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double coordinate = ConvertToDouble(value);
            if (!String.IsNullOrEmpty(FixedX))
                return new Point(ConvertToDouble(FixedX), coordinate);

            return new Point(coordinate, ConvertToDouble(FixedY));
        }

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            double x = values != null && values.Count > 0 ? ConvertToDouble(values[0]) : 0;
            double y = values != null && values.Count > 1 ? ConvertToDouble(values[1]) : 0;
            return new Point(x, y);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static double ConvertToDouble(object value)
        {
            if (value == null)
                return 0;

            try
            {
                double result = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return double.IsNaN(result) || double.IsInfinity(result) ? 0 : result;
            }
            catch (FormatException)
            {
                return 0;
            }
            catch (InvalidCastException)
            {
                return 0;
            }
            catch (OverflowException)
            {
                return 0;
            }
        }
    }
}
