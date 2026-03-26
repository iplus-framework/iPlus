using Avalonia.Data.Converters;
using System;
using System.Globalization;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui
{
    [Converter(typeof(object), typeof(bool))]
    public class IsVisibleNullConverter : IValueConverter
    {
        private static IsVisibleNullConverter _Current;

        public static IsVisibleNullConverter Current
        {
            get
            {
                if (_Current == null) _Current = new IsVisibleNullConverter();
                return _Current;
            }
        }
        /// <summary>
        /// Updated and builded method
        /// - param : 
        ///     invert -> logic - hide if value is null
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invertedLogic = parameter != null && parameter.ToString().Contains("invert");

            return value != null ^ invertedLogic;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }
    }
}
