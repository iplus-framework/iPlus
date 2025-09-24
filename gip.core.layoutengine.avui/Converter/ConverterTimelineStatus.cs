using Avalonia.Data.Converters;
using gip.core.datamodel;
using System;
using System.Collections.Generic;

namespace gip.core.layoutengine.avui
{
    public class ConverterTimelineStatus : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<Global.TimelineItemStatus> status = value as List<Global.TimelineItemStatus>;
            Global.TimelineItemStatus icon;
            Enum.TryParse(parameter.ToString(), out icon);
            if (status.Contains(icon))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
