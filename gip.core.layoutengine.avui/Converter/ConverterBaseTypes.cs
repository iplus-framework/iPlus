using Avalonia.Data.Converters;
using gip.core.datamodel;
using gip.ext.design.avui.PropertyGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    public abstract class ConverterBaseTypesBase : ConverterBase
    {
        internal abstract object ConvertToTargetType(object resultValue);
        internal virtual object ConvertToSourceType(object resultValue, Type targetType, object parameter)
        {
            return resultValue;
        }

        /// <summary>
        /// e.g. Binded object is a TimeSpan and Converter should access a Property of the TimeSpan-Struct
        /// </summary>
        public string ValueFromSubProperty { get; set; }
    }

    public abstract class ConverterBaseTypesBaseSingle : ConverterBaseTypesBase, IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConverterBaseTypesBaseMulti.Convert(this, new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = ConverterBaseTypesBaseMulti.ConvertBack(this, value, new Type[] { targetType }, parameter, culture);
            if (result == null)
                return null;
            if (!result.Any())
                return null;
            return ConvertToSourceType(value, targetType, parameter);
        }
        #endregion
    }

    public abstract class ConverterBaseTypesBaseMulti : ConverterBaseTypesBase, IMultiValueConverter
    {
        #region IMultiValueConverter Members
        public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(this, values, targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = ConvertBack(this, value, targetTypes, parameter, culture);
            if (result == null)
                return null;
            if (!result.Any())
                return result;
            object[] convresult = new object[result.Count()];
            int index = 0;
            foreach (object resVal in result)
            {
                convresult[index] = ConvertToSourceType(value, targetTypes[index], parameter);
                index++;
            }
            return convresult;
        }
        #endregion

        internal static object Convert(ConverterBaseTypesBase converter, IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = null;
            if ((converter.ConversionBy != ConvType.Direct) && (converter.Calculator != null))
            {
                ICalculatorResult calcResult = converter.Calculator.Calculate(values, targetType, parameter, culture, out result);
                if (calcResult == ICalculatorResult.FromScriptEngine)
                    return result;
                else if (calcResult == ICalculatorResult.FromExpression)
                    return converter.ConvertToTargetType(result);
            }
            if (values == null || !values.Any())
                return null;
            return converter.ConvertToTargetType(values[0]);
        }

        internal static object[] ConvertBack(ConverterBaseTypesBase converter, object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = new object[targetTypes.Count()];
            int i = 0;
            foreach (Type type in targetTypes)
            {
                result[i] = value;
            }
            return result;
        }
    }

    #region Object / Direct
    [ConverterAttribute(typeof(object), typeof(Object))]
    public class ConverterObject : ConverterBaseTypesBaseSingle
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return resultValue;
        }
    }

    [ConverterAttribute(typeof(object), typeof(Object))]
    public class ConverterObjectMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return resultValue;
        }
    }
    #endregion


    #region Boolean
    [ConverterAttribute(typeof(object), typeof(Boolean))]
    public class ConverterBoolean : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return false;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToBoolean(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterBoolean", "ConvertInternal", msg);
            }
            return false;
        }

        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterBoolean.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(Boolean))]
    public class ConverterBooleanMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterBoolean.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region Byte
    [ConverterAttribute(typeof(object), typeof(Byte))]
    public class ConverterByte : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return Byte.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToByte(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterByte", "ConvertInternal", msg);
            }
            return Byte.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterByte.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(Byte))]
    public class ConverterByteMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterByte.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region Char
    [ConverterAttribute(typeof(object), typeof(Char))]
    public class ConverterChar : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return Char.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToChar(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterChar", "ConvertInternal", msg);
            }
            return Char.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterChar.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(Char))]
    public class ConverterCharMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterChar.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region DateTime
    [ConverterAttribute(typeof(object), typeof(DateTime))]
    public class ConverterDateTime : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return DateTime.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToDateTime(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterDateTime", "ConvertInternal", msg);
            }
            return DateTime.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterDateTime.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(DateTime))]
    public class ConverterDateTimeMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterDateTime.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region TimeSpan
    [ConverterAttribute(typeof(object), typeof(TimeSpan))]
    public class ConverterTimeSpan : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return TimeSpan.MinValue;
            try
            {
                if (resultValue is IConvertible)
                {
                    return new TimeSpan((Int64)ConverterInt64.ConvertInternal(resultValue));
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterTimeSpan", "ConvertInternal", msg);
            }
            return TimeSpan.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterTimeSpan.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(TimeSpan))]
    public class ConverterTimeSpanMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterTimeSpan.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region Decimal
    [ConverterAttribute(typeof(object), typeof(Decimal))]
    public class ConverterDecimal : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return Decimal.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToDecimal(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterDecimal", "ConvertInternal", msg);
            }
            return Decimal.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterDecimal.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(Decimal))]
    public class ConverterDecimalMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterDecimal.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region Double
    [ConverterAttribute(typeof(object), typeof(Double))]
    public class ConverterDouble : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue, ConverterBaseTypesBase converter)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return Double.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToDouble(resultValue);
                else if (!String.IsNullOrEmpty(converter.ValueFromSubProperty))
                {
                    if (resultValue != null)
                    {
                        object source = resultValue;
                        PropertyInfo pi = null;
                        source = TypeAnalyser.GetPropertyPathValue(source, converter.ValueFromSubProperty, out pi);
                        return source;
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterDouble", "ConvertInternal", msg);
            }
            return Double.MinValue;
        }

        internal static object ConvertInternalBack(object resultValue, ConverterBaseTypesBase converter, Type targetType, object parameter)
        {
            return ACConvert.ChangeType(resultValue, targetType, false, Database.GlobalDatabase);
        }

        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterDouble.ConvertInternal(resultValue, this);
        }

        internal override object ConvertToSourceType(object resultValue, Type targetType, object parameter)
        {
            return ConverterDouble.ConvertInternalBack(resultValue, this, targetType, parameter);
        }

    }

    [ConverterAttribute(typeof(object), typeof(Double))]
    public class ConverterDoubleMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterDouble.ConvertInternal(resultValue, this);
        }

        internal override object ConvertToSourceType(object resultValue, Type targetType, object parameter)
        {
            return ConverterDouble.ConvertInternal(resultValue, this);
        }
    }
    #endregion

    #region Int16
    [ConverterAttribute(typeof(object), typeof(Int16))]
    public class ConverterInt16 : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return Int16.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToInt16(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterInt16", "ConvertInternal", msg);
            }
            return Int16.MinValue;
        }

        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterInt16.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(Int16))]
    public class ConverterInt16Multi : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterInt16.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region Int32
    [ConverterAttribute(typeof(object), typeof(Int32))]
    public class ConverterInt32 : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return Int32.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToInt32(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterInt32", "ConvertInternal", msg);
            }
            return Int32.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterInt32.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(Int32))]
    public class ConverterInt32Multi : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterInt32.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region Int64
    [ConverterAttribute(typeof(object), typeof(Int64))]
    public class ConverterInt64 : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return Int64.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToInt64(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterInt64", "ConvertInternal", msg);
            }
            return Int64.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterInt64.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(Int64))]
    public class ConverterInt64Multi : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterInt64.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region SByte
    [ConverterAttribute(typeof(object), typeof(SByte))]
    public class ConverterSByte : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootProjectName))
                return SByte.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToSByte(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterSByte", "ConvertInternal", msg);
            }
            return SByte.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterSByte.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(SByte))]
    public class ConverterSByteMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterSByte.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region Single
    [ConverterAttribute(typeof(object), typeof(Single))]
    public class ConverterSingle : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootClassName))
                return Single.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToSingle(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterSingle", "ConvertInternal", msg);
            }
            return Single.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterSingle.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(Single))]
    public class ConverterSingleMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterSingle.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region String
    [ConverterAttribute(typeof(object), typeof(String))]
    public class ConverterString : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootClassName))
                return "---";
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToString(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterString", "ConvertInternal", msg);
            }
            return "---";
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterString.ConvertInternal(resultValue);
        }

        internal override object ConvertToSourceType(object resultValue, Type targetType, object parameter)
        {
            return ACConvert.ChangeType(resultValue, targetType, false, Database.GlobalDatabase);
            //return resultValue;
        }

    }

    [ConverterAttribute(typeof(object), typeof(String))]
    public class ConverterStringMulti : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterString.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region UInt16
    [ConverterAttribute(typeof(object), typeof(UInt16))]
    public class ConverterUInt16 : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootClassName))
                return UInt16.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToUInt16(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterUInt16", "ConvertInternal", msg);
            }
            return UInt16.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterUInt16.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(UInt16))]
    public class ConverterUInt16Multi : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterUInt16.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region UInt32
    [ConverterAttribute(typeof(object), typeof(UInt32))]
    public class ConverterUInt32 : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootClassName))
                return UInt32.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToUInt32(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterUInt32", "ConvertInternal", msg);
            }
            return UInt32.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterUInt32.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(UInt32))]
    public class ConverterUInt32Multi : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterUInt32.ConvertInternal(resultValue);
        }
    }
    #endregion

    #region UInt64
    [ConverterAttribute(typeof(object), typeof(UInt64))]
    public class ConverterUInt64 : ConverterBaseTypesBaseSingle
    {
        internal static object ConvertInternal(object resultValue)
        {
            // Falls Binding Fehler
            if ((resultValue != null) && (resultValue is String) && (resultValue as String == Const.ACRootClassName))
                return UInt64.MinValue;
            try
            {
                if (resultValue is IConvertible)
                    return System.Convert.ToUInt64(resultValue);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterUInt64", "ConvertInternal", msg);
            }
            return UInt64.MinValue;
        }


        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterUInt64.ConvertInternal(resultValue);
        }
    }

    [ConverterAttribute(typeof(object), typeof(UInt64))]
    public class ConverterUInt64Multi : ConverterBaseTypesBaseMulti
    {
        internal override object ConvertToTargetType(object resultValue)
        {
            return ConverterUInt64.ConvertInternal(resultValue);
        }
    }
    #endregion
}
