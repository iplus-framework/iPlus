using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Reflection;
using System.Windows;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    public class ConverterAbbreviateNum : IValueConverter
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
            if (value == null || !(value is IConvertible))
                return "";
            return AbbreviateNum((value as IConvertible).ToInt64(CultureInfo.InvariantCulture));
        }

        public string AbbreviateNum(long value)
        {
            if (value <= 0)
                return "<>"; // "∞";

            if (value < 1000)
                return value.ToString();

            if (value < 10000)
                return String.Format("{0:#,.##}K", value - 5);

            if (value < 100000)
                return String.Format("{0:#,.#}K", value - 50);

            if (value < 1000000)
                return String.Format("{0:#,.}K", value - 500);

            if (value < 10000000)
                return String.Format("{0:#,,.##}M", value - 5000);

            if (value < 100000000)
                return String.Format("{0:#,,.#}M", value - 50000);

            if (value < 1000000000)
                return String.Format("{0:#,,.}M", value - 500000);

            return String.Format("{0:#,,,.##}B", value - 5000000);
            //if (value >= 100000000000)
            //    return (value / 1000000000).ToString("#,0") + "B";
            //if (value >= 10000000000)
            //    return (value / 1000000000D).ToString("0.#") + "B";
            //if (value >= 100000000)
            //    return (value / 1000000).ToString("#,0") + "M";
            //if (value >= 10000000)
            //    return (value / 1000000D).ToString("0.#") + "M";
            //if (value >= 100000)
            //    return (value / 10000).ToString("#,0") + "K";
            //if (value >= 10000)
            //    return (value / 1000D).ToString("0.#") + "K";
            //if (value >= 1000)
            //    return (value / 1000).ToString("#") + "K";
            //return value.ToString("#,0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
