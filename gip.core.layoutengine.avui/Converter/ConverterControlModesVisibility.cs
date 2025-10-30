using Avalonia.Data.Converters;
using gip.core.datamodel;
using System;
using System.Globalization;

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
                    case Global.ControlModes.Hidden:
                        return false;
                    default:
                        return true;
                }
            }
            catch  (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConverterControlModesVisibility", "IValueConverter.Convert", msg);

                return Visibility.Collapsed;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
