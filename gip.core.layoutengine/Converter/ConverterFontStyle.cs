﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using gip.core.datamodel;
using gip.ext.design.PropertyGrid;

namespace gip.core.layoutengine
{
    [ConverterAttribute(typeof(object), typeof(FontStyle))]
    public class ConverterFontStyleSingle : ConverterBase, IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConverterFontStyleMulti.Convert(this, new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = ConverterFontStyleMulti.ConvertBack(this, value, new Type[] { targetType }, parameter, culture);
            if (result == null)
                return null;
            if (!result.Any())
                return null;
            return result[0];
        }
        #endregion
    }

    [ConverterAttribute(typeof(object), typeof(FontStyle))]
    public class ConverterFontStyleMulti : ConverterBase, IMultiValueConverter
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
                        if (result is FontStyle)
                            return result;

                    }
                    else if (calcResult == ICalculatorResult.FromExpression)
                    {
                        if (result is String)
                        {
                            FontStyleConverter fontConv = new FontStyleConverter();
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
                        FontStyleConverter fontConv = new FontStyleConverter();
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
                    datamodel.Database.Root.Messages.LogException("ConverterFontStyleMulti", "Convert", msg);
            }
            if (val <= 0)
                return FontStyles.Normal;
            else if (val == 1)
                return FontStyles.Italic;
            return FontStyles.Oblique;
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
