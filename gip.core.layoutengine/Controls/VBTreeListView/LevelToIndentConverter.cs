using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace gip.core.layoutengine
{
    /// <summary>
    /// The converter for level to indent.
    /// </summary>
    public class LevelToIndentConverter : IValueConverter
    {
        /// <summary>
        /// Converts level to indent.
        /// </summary>
        /// <param name="o">The level.</param>
        /// <param name="type">The type.</param>
        /// <param name="parameter">The indent size.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The indent size according level.</returns>
        public object Convert(object o, Type type, object parameter, CultureInfo culture)
        {
            Double indentSize = 0;
            if (parameter != null)
                Double.TryParse(parameter.ToString(), out indentSize);

            return ((int)o) * indentSize;
            //else
            //    return new Thickness((int)o * c_IndentSize, 0, 0, 0);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="type"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

}
