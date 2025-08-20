using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using gip.core.datamodel;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui
{
    [ConverterAttribute(typeof(object), typeof(FontWeight))]
    public class ConverterFontWeightSingle : ConverterBase, IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConverterFontWeightMulti.Convert(this, new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = ConverterFontWeightMulti.ConvertBack(this, value, new Type[] { targetType }, parameter, culture);
            if (result == null)
                return null;
            if (!result.Any())
                return null;
            return result[0];
        }
        #endregion
    }

    [ConverterAttribute(typeof(object), typeof(FontWeight))]
    public class ConverterFontWeightMulti : ConverterBase, IMultiValueConverter
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
            byte val = 0;
            try
            {
                if ((converter.ConversionBy != ConvType.Direct) && (converter.Calculator != null))
                {
                    ICalculatorResult calcResult = converter.Calculator.Calculate(values, targetType, parameter, culture, out result);
                    if (calcResult == ICalculatorResult.FromScriptEngine)
                    {
                        if (result is FontWeight)
                            return result;
                    }
                    else if (calcResult == ICalculatorResult.FromExpression)
                    {
                        if (result is String)
                        {
                            FontWeightConverter fontConv = new FontWeightConverter();
                            return fontConv.ConvertFromString(result as String);
                        }
                        else if (result is IConvertible)
                            val = System.Convert.ToByte(result);
                    }
                    else if (values[0] is IConvertible)
                        val = System.Convert.ToByte(values[0]);
                }
                else
                {
                    if (values[0] is String)
                    {
                        FontWeightConverter fontConv = new FontWeightConverter();
                        return fontConv.ConvertFromString(values[0] as String);
                    }
                    else if (values[0] is IConvertible)
                        val = System.Convert.ToByte(values[0]);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterFontWeightMulti", "Convert", msg);
            }
            switch (val)
            {
                case 0:
                case 1:
                    return FontWeights.Black;
                case 2:
                    return FontWeights.Bold;
                case 3:
                    return FontWeights.DemiBold;
                case 4:
                    return FontWeights.ExtraBlack;
                case 5:
                    return FontWeights.ExtraBold;
                case 6:
                    return FontWeights.ExtraLight;
                case 7:
                    return FontWeights.Heavy;
                case 8:
                    return FontWeights.Light;
                case 9:
                    return FontWeights.Medium;
                case 10:
                    return FontWeights.Normal;
                case 11:
                    return FontWeights.Regular;
                case 12:
                    return FontWeights.SemiBold;
                case 13:
                    return FontWeights.Thin;
                case 14:
                    return FontWeights.UltraBlack;
                case 15:
                    return FontWeights.UltraBold;
                case 16:
                    return FontWeights.UltraLight;
                default:
                    return FontWeights.Normal;
            }
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
