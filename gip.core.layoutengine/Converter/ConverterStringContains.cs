using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Reflection;
using System.Windows;
using System.Globalization;

namespace gip.core.layoutengine
{
    public class ConverterStringContains : IValueConverter
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
                return "";

            string valueString = value as string;
            if (string.IsNullOrEmpty(valueString) || (parameter == null))
                return false;
            string parameterAsString = parameter.ToString();
            if (String.IsNullOrEmpty(parameterAsString))
                return false;
            if (parameterAsString[0] == '$' && parameterAsString.Contains(";"))
            {
                string[] searchVals = parameterAsString.Substring(1).Split(';');
                foreach (string searchVal in searchVals)
                {
                    if (!String.IsNullOrEmpty(searchVal) && valueString.Contains(searchVal))
                        return true;
                }
                return false;
            }
            else
                return valueString.Contains(parameterAsString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
