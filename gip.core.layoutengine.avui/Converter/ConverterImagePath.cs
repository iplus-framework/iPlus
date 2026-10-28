using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using gip.ext.design.avui.PropertyGrid;
using System;
using System.Globalization;
using System.IO;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Converts image bytes (preferred, e.g. from the ACMediaService) or a local file path to a
    /// <see cref="Bitmap"/> so that an Image control can display it. Avalonia's binding engine has
    /// no built-in conversion to Bitmap at runtime, therefore this converter is required.
    /// </summary>
    [ConverterAttribute(typeof(object), typeof(Bitmap))]
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
            try
            {
                // Content received from the media service: works without any file system access
                if (value is byte[] content)
                {
                    if (content.Length == 0)
                        return null;
                    using (MemoryStream stream = new MemoryStream(content))
                        return new Bitmap(stream);
                }

                string filePath = value as string;
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return null;
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
