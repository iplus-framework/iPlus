using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{
	public class FormattedMultiTextConverter : IMultiValueConverter
	{
		#region IMultiValueConverter Members

		public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			return string.Format( (string) parameter, values );
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
		{
			return null;
		}

        #endregion
    }

	public class FormattedTextConverter : IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format((string)parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
