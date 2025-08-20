using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using gip.core.datamodel;
using System.Windows.Media;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui
{
    [ConverterAttribute(typeof(object), typeof(FontFamily))]
    public class ConverterFontFamilySingle : ConverterBase, IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConverterFontFamilyMulti.Convert(this, new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = ConverterFontFamilyMulti.ConvertBack(this, value, new Type[] { targetType }, parameter, culture);
            if (result == null)
                return null;
            if (!result.Any())
                return null;
            return result[0];
        }
        #endregion
    }

    [ConverterAttribute(typeof(object), typeof(FontFamily))]
    public class ConverterFontFamilyMulti : ConverterBase, IMultiValueConverter
    {
        #region IMultiValueConverter Members
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(this, values, targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertBack(this, value, targetTypes, parameter, culture);
        }
        #endregion

        internal static object Convert(ConverterBase converter, object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = null;
            if ((converter.ConversionBy != ConvType.Direct) && (converter.Calculator != null))
            {
                ICalculatorResult calcResult = converter.Calculator.Calculate(values, targetType, parameter, culture, out result);
                if (calcResult == ICalculatorResult.FromScriptEngine)
                {
                    if (result is FontFamily)
                        return result;
                    else
                        return Fonts.SystemFontFamilies.First();
                }
                else if (calcResult == ICalculatorResult.FromExpression)
                {
                    if (result is String)
                    {
                        String familiyName = result as String;
                        if (!String.IsNullOrEmpty(familiyName))
                        {
                            FontFamilyConverter typeConverter = new FontFamilyConverter();
                            return typeConverter.ConvertFromString(familiyName);
                        }
                    }
                    return Fonts.SystemFontFamilies.First();
                }
            }
            if ((values != null) && (values.Any()))
            {
                try
                {
                    if (values[0] is String)
                    {
                        String familiyName = values[0] as String;
                        if (!String.IsNullOrEmpty(familiyName))
                        {
                            FontFamilyConverter typeConverter = new FontFamilyConverter();
                            return typeConverter.ConvertFromString(familiyName);
                        }
                    }
                    else if (values[0] is IConvertible)
                    {
                        int fontIndex = System.Convert.ToInt32(values[0]);
                        if (fontIndex < 0)
                            fontIndex = 0;
                        if (Fonts.SystemFontFamilies.Count >= (fontIndex + 1))
                            return Fonts.SystemFontFamilies.ElementAt(fontIndex);
                        else
                            return Fonts.SystemFontFamilies.Last();
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ConverterFontFamilyMulti", "Convert", msg);
                }
            }
            return Fonts.SystemFontFamilies.First();
        }

        internal static object[] ConvertBack(ConverterBase converter, object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = new object[targetTypes.Count()];
            int i = 0;
            foreach (Type type in targetTypes)
            {
                result[i] = null;
            }
            return result;
        }
    }
}
