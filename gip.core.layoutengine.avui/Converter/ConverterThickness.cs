using Avalonia;
using Avalonia.Data.Converters;
using gip.core.datamodel;
using gip.ext.design.avui.PropertyGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    [ConverterAttribute(typeof(object), typeof(Thickness))]
    public class ConverterThicknessSingle : ConverterBase, IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConverterThicknessMulti.Convert(this, new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = ConverterThicknessMulti.ConvertBack(this, value, new Type[] { targetType }, parameter, culture);
            if (result == null)
                return null;
            if (!result.Any())
                return null;
            return result[0];
        }
        #endregion
    }

    [ConverterAttribute(typeof(object), typeof(Thickness))]
    public class ConverterThicknessMulti : ConverterBase, IMultiValueConverter
    {
        #region IMultiValueConverter Members
        public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(this, values, targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertBack(this, value, targetTypes, parameter, culture);
        }
        #endregion

        internal static object Convert(ConverterBase converter, IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = null;
            double dThickness = 1;
            try
            {
                if ((converter.ConversionBy != ConvType.Direct) && (converter.Calculator != null))
                {
                    ICalculatorResult calcResult = converter.Calculator.Calculate(values, targetType, parameter, culture, out result);
                    if (calcResult == ICalculatorResult.FromScriptEngine)
                    {
                        if (result is Thickness)
                            return result;
                    }
                    else if (calcResult == ICalculatorResult.FromExpression)
                    {
                        if (result is String)
                        {
                            return Thickness.Parse(result as String);
                        }
                        else if (result is IConvertible)
                            return new Thickness(System.Convert.ToDouble(result));
                    }
                    else if (values[0] is IConvertible)
                        dThickness = System.Convert.ToDouble(values[0]);
                }
                else if (values[0] is IConvertible)
                    dThickness = System.Convert.ToDouble(values[0]);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterThicknessMulti", "Convert", msg);
            }
            return new Thickness(dThickness);
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
