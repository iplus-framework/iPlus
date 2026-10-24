using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using gip.ext.design.avui.PropertyGrid;
using System;
using System.Globalization;
using System.IO;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Converts a file path to a <see cref="Bitmap"/> so that an Image control can display
    /// media files that are stored on the server. Avalonia's binding engine has no built-in
    /// string-to-Bitmap conversion at runtime, therefore this converter is required.
    /// </summary>
    [ConverterAttribute(typeof(string), typeof(Bitmap))]
    public class ConverterImagePath : IValueConverter
    {
        #region Singleton
        private static ConverterImagePath _Current;

        public static ConverterImagePath Current
        {
            get
            {
                if (_Current == null)
                    _Current = new ConverterImagePath();
                return _Current;
            }
        }
        #endregion

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string filePath = value as string;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                return new Bitmap(filePath);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
