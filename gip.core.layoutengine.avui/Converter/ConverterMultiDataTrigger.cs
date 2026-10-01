using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Multi-value converter used by converted WPF MultiDataTriggers (XAMLConversionHelper).
    /// Compares each child binding result against the expected value passed as
    /// ConverterParameter (comma separated, e.g. "False,False,True").
    /// Returns true only if all values match their expected value.
    /// Empty or "True"/"1" entries expect a boolean true, "False"/"0" expect boolean false.
    /// Any other entry is compared with ObjectEqualsConverter.AreEquivalent.
    /// </summary>
    public class ConverterMultiDataTrigger : IMultiValueConverter
    {
        private static ConverterMultiDataTrigger _Current;

        public static ConverterMultiDataTrigger Current
        {
            get
            {
                if (_Current == null)
                    _Current = new ConverterMultiDataTrigger();
                return _Current;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            string expectedList = parameter as string;
            if (string.IsNullOrEmpty(expectedList))
                return true;

            string[] expected = expectedList.Split(',');
            for (int i = 0; i < expected.Length; i++)
            {
                object value = (values != null && i < values.Count) ? values[i] : null;
                string exp = expected[i].Trim();

                bool result;
                if (string.IsNullOrEmpty(exp) ||
                    string.Equals(exp, "True", StringComparison.OrdinalIgnoreCase) ||
                    exp == "1")
                {
                    result = IsTrueValue(value);
                }
                else if (string.Equals(exp, "False", StringComparison.OrdinalIgnoreCase) || exp == "0")
                {
                    result = !IsTrueValue(value);
                }
                else
                {
                    result = ObjectEqualsConverter.AreEquivalent(value, exp, culture);
                }

                if (!result)
                    return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static bool IsTrueValue(object value)
        {
            if (value is bool boolValue)
                return boolValue;
            if (value == null)
                return false;
            return bool.TryParse(value.ToString(), out bool parsed) && parsed;
        }
    }
}
