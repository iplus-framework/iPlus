using Avalonia.Data.Converters;
using System;

namespace gip.core.layoutengine.avui
{
    public class ConverterStringIsNullOrEmpty : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Returns Boolean
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string))
                return true;

            string valueItem = value as string;

            return string.IsNullOrEmpty(valueItem);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
