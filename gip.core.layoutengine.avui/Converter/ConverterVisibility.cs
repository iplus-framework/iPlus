using Avalonia.Data.Converters;
using gip.core.datamodel;
using gip.ext.design.avui.PropertyGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Old WPF Complatibility Enum for Visibility
    /// </summary>
    public enum Visibility : byte
    {
        Visible = 0,
        Hidden,
        Collapsed
    }

    [ConverterAttribute(typeof(object), typeof(Visibility))]
    public class ConverterVisibilitySingle : ConverterBase, IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = false;
            Visibility visRes = (Visibility) ConverterVisibilityMulti.Convert(this, new object[] { value }, targetType, parameter, culture);
            if (Negate)
            {
                if (visRes == Visibility.Visible)
                    isVisible = false;
                else if (visRes == Visibility.Hidden)
                    isVisible = true;
            }
            if (UseCollapsed && (visRes == Visibility.Hidden))
                isVisible = false;
            return isVisible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = ConverterVisibilityMulti.ConvertBack(this, value, new Type[] { targetType }, parameter, culture);
            if (result == null)
                return null;
            if (!result.Any())
                return null;
            return result[0];
        }
        #endregion

        public bool Negate
        {
            get;set;
        }

        public bool UseCollapsed
        {
            get;
            set;
        }
    }

    [ConverterAttribute(typeof(object), typeof(Visibility))]
    public class ConverterVisibilityMulti : ConverterBase, IMultiValueConverter
    {
        #region IMultiValueConverter Members
        public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = false;
            Visibility visRes = (Visibility)Convert(this, values, targetType, parameter, culture);
            if (Negate)
            {
                if (visRes == Visibility.Visible)
                    isVisible = false;
                else if (visRes == Visibility.Hidden)
                    isVisible = true;
            }
            if (UseCollapsed && (visRes == Visibility.Hidden))
                isVisible = false;
            return isVisible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertBack(this, value, targetTypes, parameter, culture);
        }
        #endregion

        internal static object Convert(ConverterBase converter, IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
                        if (result is Visibility visibility)
                            return visibility == Visibility.Visible ? true : false;
                    }
                    else if (calcResult == ICalculatorResult.FromExpression)
                    {
                        if (result is String)
                        {
                            String text = result as String;
                            if (text == "Visible")
                                return true;
                            else if (text == "Collapsed")
                                return false;
                            return false;
                        }
                        else if (result is IConvertible)
                            val = System.Convert.ToByte(result);
                    }
                    else if (values[0] is IConvertible)
                    {
                        if ((values[0] is String))
                            byte.TryParse((String)values[0], out val);
                        else
                            val = System.Convert.ToByte(values[0]);
                    }
                }
                else if (values[0] is IConvertible)
                {
                    if ((values[0] is String))
                        byte.TryParse((String)values[0], out val);
                    else
                        val = System.Convert.ToByte(values[0]);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterVisibiltyMulti", "Convert", msg);
            }
            if (val == 0)
                return true;
            else if (val == 1)
                return false;
            return false;
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

        public bool Negate
        {
            get;set;
        }

        public bool UseCollapsed
        {
            get;
            set;
        }
    }
}
