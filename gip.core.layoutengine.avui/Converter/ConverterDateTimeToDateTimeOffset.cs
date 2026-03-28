using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Converts between DateTime and DateTimeOffset for DatePicker binding compatibility.
    /// </summary>
    public class ConverterDateTimeToDateTimeOffset : IValueConverter
    {
        public static readonly ConverterDateTimeToDateTimeOffset Instance = new();

        private static ConverterDateTimeToDateTimeOffset _Current;

        public static ConverterDateTimeToDateTimeOffset Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new ConverterDateTimeToDateTimeOffset();
                return _Current;
            }
        }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                DateTime dt => new DateTimeOffset(dt),
                DateTimeOffset dto => dto,
                null => null,
                _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateTimeOffset")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset dto)
            {
                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    return dto.DateTime;
                return dto;
            }
            return null;
        }
    }
}