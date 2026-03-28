using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{
	public class FormattedMultiTextConverter : IMultiValueConverter
	{
        #region Singleton
        private static FormattedMultiTextConverter _Current;

        public static FormattedMultiTextConverter Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new FormattedMultiTextConverter();
                return _Current;
            }
        }
        #endregion

		#region IMultiValueConverter Members

		public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			return string.Format( (string) parameter, values );
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
		{
			return null;
		}

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

        #endregion
    }

	public class FormattedTextConverter : IValueConverter
    {
        #region Singleton
        private static FormattedTextConverter _Current;

        public static FormattedTextConverter Current
        {
            get
            {
                if (_Current == null) 
                    _Current = new FormattedTextConverter();
                return _Current;
            }
        }
        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format((string)parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return Current;
        }

        #endregion
    }
}
