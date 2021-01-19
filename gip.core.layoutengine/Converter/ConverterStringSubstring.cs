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
    public class ConverterStringSubstring : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Returns a Substring
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
            if (string.IsNullOrEmpty(valueString))
                return "";
            if (parameter != null)
            {
                int substrFrom = 0;
                int substrLength = 0;
                string parameterAsString = parameter.ToString();
                if (parameterAsString.Length > 1)
                {
                    if (parameterAsString.IndexOf(",") > 0)
                    {
                        string[] fromto = parameterAsString.Split(',');
                        if (fromto.Length >= 2)
                        {
                            substrFrom = System.Convert.ToInt32(fromto[0]);
                            substrLength = System.Convert.ToInt32(fromto[1]);
                        }
                        else
                            substrLength = System.Convert.ToInt32(fromto[0]);
                    }
                    else
                        substrLength = System.Convert.ToInt32(parameterAsString);
                }
                else
                {
                    substrLength = System.Convert.ToInt32(parameterAsString);
                }

                if (valueString.Length >= (substrFrom + substrLength))
                    return valueString.Substring(substrFrom, substrLength);
                else
                    return valueString;
            }
            return valueString.Substring(0, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
