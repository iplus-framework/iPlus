using Avalonia.Data.Converters;
using gip.core.datamodel;
using System;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Converts a boolean value to a <see cref="Global.ControlModes"/> mode.
    /// Set <see cref="TrueMode"/> and <see cref="FalseMode"/> to the desired mode names
    /// (e.g. "Disabled", "Enabled").
    /// </summary>
    public class ConverterBoolControlMode : IValueConverter
    {
        private static ConverterBoolControlMode _Current;

        public static ConverterBoolControlMode Current
        {
            get
            {
                if (_Current == null)
                    _Current = new ConverterBoolControlMode();
                return _Current;
            }
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

        public string TrueMode { get; set; } = "Disabled";
        public string FalseMode { get; set; } = "Enabled";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = value is bool b && b;
            string modeName = isTrue ? TrueMode : FalseMode;
            if (string.IsNullOrWhiteSpace(modeName) ||
                !Enum.TryParse(modeName, true, out Global.ControlModes mode))
                return Avalonia.Data.BindingOperations.DoNothing;
            return mode;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}
