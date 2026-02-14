using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Converts TreeViewItem level to left margin (Thickness) for proper indentation.
    /// </summary>
    public class TreeViewItemLevelToMarginConverter : IValueConverter
    {
        private const double DefaultIndentSize = 16.0;

        /// <summary>
        /// Gets the singleton instance of the converter.
        /// </summary>
        public static readonly TreeViewItemLevelToMarginConverter Instance = new TreeViewItemLevelToMarginConverter();

        /// <summary>
        /// Converts level to margin thickness.
        /// </summary>
        /// <param name="value">The level value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The indent size (optional, defaults to 16).</param>
        /// <param name="culture">The culture.</param>
        /// <returns>A Thickness with left margin based on level.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int level)
                return new Thickness(0);

            double indentSize = DefaultIndentSize;
            if (parameter != null)
            {
                if (parameter is double doubleParam)
                    indentSize = doubleParam;
                else if (parameter is string stringParam)
                    double.TryParse(stringParam, out indentSize);
            }

            return new Thickness(level * indentSize, 0, 0, 0);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
