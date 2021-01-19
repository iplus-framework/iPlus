using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using gip.core.datamodel;
using System.Windows.Media;
using gip.ext.design.PropertyGrid;

namespace gip.core.layoutengine
{
    [ConverterAttribute(typeof(object), typeof(Brush))]
    public class ConverterBrushSingle : ConverterBase, IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConverterBrushMulti.Convert(this, new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = ConverterBrushMulti.ConvertBack(this, value, new Type[] { targetType }, parameter, culture);
            if (result == null)
                return null;
            if (!result.Any())
                return null;
            return result[0];
        }
        #endregion
    }

    [ConverterAttribute(typeof(object), typeof(Brush))]
    public class ConverterBrushMulti : ConverterBase, IMultiValueConverter
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
                    if (result is Brush)
                        return result;
                    else
                        return ErrorBrush;
                }
                else if (calcResult == ICalculatorResult.FromExpression)
                {
                    if (result is String)
                    {
                        BrushConverter typeConverter = new BrushConverter();
                        return typeConverter.ConvertFromString(result as String);
                    }
                    Byte val = System.Convert.ToByte(result);
                    Color c = Color.FromRgb(val, val, val);
                    return new SolidColorBrush(c);
                }
            }
            if ((values != null) && (values.Any()))
            {
                try
                {
                    if ((values[0] is double) && (values.Count() >= 3))
                    {
                        if (values.Count() == 3)
                        {
                            byte r = System.Convert.ToByte(Math.Min(System.Convert.ToDouble(values[0]), 255));
                            byte g = System.Convert.ToByte(Math.Min(System.Convert.ToDouble(values[1]), 255));
                            byte b = System.Convert.ToByte(Math.Min(System.Convert.ToDouble(values[2]), 255));
                            Color c = Color.FromRgb(r, g, b);
                            return new SolidColorBrush(c);
                        }
                        else if (values.Count() >= 4)
                        {
                            byte a = System.Convert.ToByte(Math.Min(System.Convert.ToDouble(values[0]), 255));
                            byte r = System.Convert.ToByte(Math.Min(System.Convert.ToDouble(values[1]), 255));
                            byte g = System.Convert.ToByte(Math.Min(System.Convert.ToDouble(values[2]), 255));
                            byte b = System.Convert.ToByte(Math.Min(System.Convert.ToDouble(values[3]), 255));
                            Color c = Color.FromArgb(a, r, g, b);
                            return new SolidColorBrush(c);
                        }
                    }
                    else if (values[0] is String)
                    {
                        // Falls nicht Binding-Fehler
                        if (values[0] as String != Const.ACRootClassName)
                        {
                            BrushConverter typeConverter = new BrushConverter();
                            return typeConverter.ConvertFromString(values[0] as String);
                        }
                    }
                    else if (values[0] is IConvertible)
                    {
                        Byte val = System.Convert.ToByte(Math.Min(System.Convert.ToDouble(values[0]), 255));
                        Color c = Color.FromRgb(val, val, val);
                        return new SolidColorBrush(c);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ConverterBrushMulti", "Convert", msg);
                }
            }
            return ErrorBrush;
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

        private static Brush _ErrorBrush;
        private static Brush ErrorBrush
        {
            get
            {
                if (_ErrorBrush == null)
                {
                    ResourceDictionary dict = new ResourceDictionary();
                    dict.Source = new Uri("/gip.core.layoutengine;Component/Controls/Shared.xaml", UriKind.Relative);
                    _ErrorBrush = (DrawingBrush)dict["ConverterErrorBrush"];
                }
                if (_ErrorBrush == null)
                    _ErrorBrush = Brushes.Red;
                return _ErrorBrush;
            }
        }
    }
}
