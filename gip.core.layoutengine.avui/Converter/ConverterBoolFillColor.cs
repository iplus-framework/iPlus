using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Converts a boolean value to a <see cref="SolidColorBrush"/>.
    /// Set <see cref="TrueColor"/> and <see cref="FalseColor"/> to the desired hex color strings (e.g. "#00FF00").
    /// </summary>
    public class ConverterBoolFillColor : IValueConverter
    {
        public string TrueColor { get; set; } = "#00FF00";
        public string FalseColor { get; set; } = "#FFFF00";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = value is bool b && b;
            string colorHex = isTrue ? TrueColor : FalseColor;
            return new SolidColorBrush(Color.Parse(colorHex));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}
