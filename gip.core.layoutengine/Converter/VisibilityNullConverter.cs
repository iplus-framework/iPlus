﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Reflection;
using System.Windows;
using System.Globalization;

namespace gip.core.layoutengine
{
    public class VisibilityNullConverter : IValueConverter
    {
        private static VisibilityNullConverter _Current;

        public static VisibilityNullConverter Current
        {
            get
            {
                if (_Current == null) _Current = new VisibilityNullConverter();
                return _Current;
            }
        }
        /// <summary>
        /// Updated and builded method
        /// - param : 
        ///     true -> for visibility hidden 
        ///     true, invert -> hidden and inverted logic - hidde if value is null
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            Visibility notPresentedVisibility = Visibility.Collapsed;
            if(parameter != null && parameter.ToString().Contains("true"))
                notPresentedVisibility = Visibility.Hidden;
            bool invertedLogic = parameter != null && parameter.ToString().Contains("invert");

            return value != null ^ invertedLogic ? Visibility.Visible : notPresentedVisibility;

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
