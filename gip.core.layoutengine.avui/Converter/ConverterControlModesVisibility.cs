using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Reflection;
using System.Windows;
using System.Globalization;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    public class ConverterControlModesVisibility : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch ((Global.ControlModes)value)
                {
                    case Global.ControlModes.Collapsed:
                        return Visibility.Collapsed;
                    case Global.ControlModes.Hidden:
                        return Visibility.Hidden;
                    default:
                        return Visibility.Visible;
                }
            }
            catch  (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterControlModesVisibility", "IValueConverter.Convert", msg);

                return System.Windows.Visibility.Collapsed;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
