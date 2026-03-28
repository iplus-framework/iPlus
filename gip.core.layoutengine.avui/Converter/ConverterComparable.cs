using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    public class IntegerEqualsOrGreaterThan : IValueConverter
    {
        private static IntegerEqualsOrGreaterThan _Current;

        public static IntegerEqualsOrGreaterThan Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new IntegerEqualsOrGreaterThan();
                return _Current;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return true;
            if (!(value is int) || !(parameter is string))
                return true;
            double intValue = (int)value;
            double compareToValue = int.Parse(parameter as string);
    
            return intValue >= compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }
    }

    public class IntegerEqualsOrLessThan : IValueConverter
    {
        private static IntegerEqualsOrLessThan _Current;

        public static IntegerEqualsOrLessThan Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new IntegerEqualsOrLessThan();
                return _Current;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return true;
            if (!(value is int) || !(parameter is string))
                return true;
            int intValue = (int)value;
            int compareToValue = int.Parse(parameter as string);

            return intValue <= compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }
    }

    public class IntegerEqualsOrGreatherThan : IValueConverter
    {
        private static IntegerEqualsOrGreatherThan _Current;

        public static IntegerEqualsOrGreatherThan Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new IntegerEqualsOrGreatherThan();
                return _Current;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return true;
            if (!(value is int) || !(parameter is string))
                return true;
            int intValue = (int)value;
            int compareToValue = int.Parse(parameter as string);

            return intValue >= compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }
    }

    public class DoubleEqualsOrGreaterThan : IValueConverter
    {
        private static DoubleEqualsOrGreaterThan _Current;

        public static DoubleEqualsOrGreaterThan Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new DoubleEqualsOrGreaterThan();
                return _Current;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return true;
            if (!(value is double) || !(parameter is string))
                return true;
            double doubleValue = (double)value;
            double compareToValue = double.Parse(parameter as string);

            return doubleValue >= compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }
    }

    public class DoubleEqualsOrLessThan : IValueConverter
    {
        private static DoubleEqualsOrLessThan _Current;

        public static DoubleEqualsOrLessThan Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new DoubleEqualsOrLessThan();
                return _Current;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return true;
            if (!(value is double) || !(parameter is string))
                return true;
            double doubleValue = (double)value;
            double compareToValue = double.Parse(parameter as string);

            return doubleValue <= compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }
    }


    public class ObjectEqualsConverter : IValueConverter
    {
        private static ObjectEqualsConverter _Current;

        public static ObjectEqualsConverter Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new ObjectEqualsConverter();
                return _Current;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null) return true;
            if (value == null || parameter == null) return false;
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }

    public class ObjectEqualsVisbilityConverter : IValueConverter
    {
        private static ObjectEqualsVisbilityConverter _Current;

        public static ObjectEqualsVisbilityConverter Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new ObjectEqualsVisbilityConverter();
                return _Current;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) 
                return false;
            return value.Equals(parameter) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
